using API.Application.Interfaces;
using API.Common.Security;
using API.Data.Interfaces;
using API.Models.DTOs.Auth;
using API.Models.Entities;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidRefreshTokenMessage = "Refresh token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.";
        private const string RotatedReason = "Refresh token đã được thay thế bằng một token mới (rotate).";

        // Cửa sổ ân hạn: SPA hay bắn nhiều request song song, khi access token hết hạn có thể có
        // 2 request refresh gần như cùng lúc bằng cùng 1 token. Request thứ 2 đến sau khi token đã
        // rotate ở request thứ 1 KHÔNG phải trộm token — chỉ từ chối nó, không revoke cả phiên.
        private const int RotationGraceSeconds = 30;

        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly JwtSettings _jwtSettings;
        private readonly RefreshTokenSettings _refreshTokenSettings;

        public AuthService(IUnitOfWork unitOfWork, JwtTokenHelper jwtTokenHelper, 
            IOptions<JwtSettings> jwtSettings, IOptions<RefreshTokenSettings> refreshTokenSettings)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenHelper = jwtTokenHelper;            
            _jwtSettings = jwtSettings.Value;
            _refreshTokenSettings = refreshTokenSettings.Value;
        }

        // --------------------------------------------------------------------
        // 🔹 LOGIN
        // --------------------------------------------------------------------
        public async Task<AuthResponse> LoginAsync(string username, string password, string? device, string ip, CancellationToken ct)
        {
            // 1. Tìm User trong Database theo Username
            var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.UserName == username, ct);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Sai tên đăng nhập hoặc mật khẩu");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Tài khoản đã bị khóa");
            }

            // 2. Kiểm tra mật khẩu bằng BCrypt
            // BCrypt.Verify sẽ tự động so sánh mật khẩu thô với chuỗi Hash trong DB
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Sai tên đăng nhập hoặc mật khẩu");
            }

            string[] roles = [user.Role];

            // 3️. Tạo Access Token
            var accessToken = _jwtTokenHelper.GenerateAccessToken(user.Id, user.UserName, roles);
            var accessExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            // 4️. Tạo Refresh Token            
            var refreshTokenEntity = BuildRefreshToken(user.Id, ip, out string plaintextRefreshToken);

            // 5. Lưu Refresh Token vào CSDL
            await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshTokenEntity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // 6. Trả kết quả
            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpires,
                RefreshToken = plaintextRefreshToken, //_refreshTokenSettings.UseCookie ? null : plaintextRefreshToken,
                RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
            };
        }

        // --------------------------------------------------------------------
        // 🔹 REFRESH TOKEN - xoay vòng (rotate) token + phát hiện token bị dùng lại (reuse detection)
        // --------------------------------------------------------------------
        public async Task<AuthResponse> RefreshAsync(string refreshToken, string ip, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // 1. Hash token nhận được từ client để tìm trong DB
            var hashedToken = JwtTokenHelper.HashToken(refreshToken);
            var refreshTokenExisting = await _unitOfWork.Repository<RefreshToken>().Query()
                .FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken, ct);

            if (refreshTokenExisting is null || refreshTokenExisting.ExpiresAt <= now)
            {
                // Nếu token không tồn tại hoặc đã hết hạn, trả về lỗi
                throw new UnauthorizedAccessException(InvalidRefreshTokenMessage);
            }

            // 2. Xử lý trường hợp Token đã bị Revoke (Kiểm tra reuse detection)
            if (refreshTokenExisting.RevokedAt is not null)
            {
                // Kiểm tra xem token này trước đó bị thu hồi có phải do xoay vòng (rotate) hay không.
                // (Nếu token bị revoke vì lý do khác như đăng xuất thì không phải dấu hiệu bị đánh cắp).
                var wasRotated = refreshTokenExisting.ReasonRevoked == RotatedReason;

                // "Tranh chấp lành tính" (Benign Race Condition): Token vừa mới bị rotate cách đây vài giây
                // (trong khoảng thời gian grace period), thường do client bị chậm mạng gửi đúp 2 request cùng lúc.
                var isBenignRace =
                    wasRotated &&
                    refreshTokenExisting.RevokedAt.Value > now.AddSeconds(-RotationGraceSeconds);

                // Nếu token bị rotate từ trước, nhưng đã quá thời gian ân hạn mà vẫn có request gửi lên -> Nghi ngờ bị đánh cắp!
                // Tiến hành vô hiệu hóa toàn bộ token của user này để bảo vệ tài khoản.
                if (wasRotated && !isBenignRace)
                {
                    await RevokeAllActiveTokensAsync(
                        refreshTokenExisting.UserId, ip, "Phát hiện dùng lại refresh token đã rotate (nghi ngờ bị đánh cắp)", ct);
                }

                // Dù là lỗi mạng lành tính hay kẻ tấn công thực sự, token cũ này đều không được cấp lại Access Token mới.
                throw new UnauthorizedAccessException(InvalidRefreshTokenMessage);
            }

            // 3. Kiểm tra thông tin User (tồn tại và trạng thái kích hoạt)
            var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == refreshTokenExisting.UserId, ct);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Tài khoản không tồn tại hoặc đã bị khóa");
            }

            // 4. ROTATE TOKEN: Build token mới, update token cũ
            var newRefreshTokenEntity = BuildRefreshToken(user.Id, ip, out string plaintextNewRefreshToken);

            refreshTokenExisting.RevokedAt = now;
            refreshTokenExisting.RevokedByIp = ip;
            refreshTokenExisting.ReasonRevoked = RotatedReason;

            _unitOfWork.Repository<RefreshToken>().Update(refreshTokenExisting);
            await _unitOfWork.Repository<RefreshToken>().AddAsync(newRefreshTokenEntity, ct);

            // Lưu lần 1 để sinh ID cho newRefreshTokenEntity
            await _unitOfWork.SaveChangesAsync(ct);

            // Cập nhật liên kết truy vết chuỗi token (ReplacedByTokenId)
            refreshTokenExisting.ReplacedByTokenId = newRefreshTokenEntity.Id;

            _unitOfWork.Repository<RefreshToken>().Update(refreshTokenExisting);
            await _unitOfWork.SaveChangesAsync(ct);

            // 5. Sinh Access Token mới và trả về response
            string[] roles = [user.Role];
            var accessToken = _jwtTokenHelper.GenerateAccessToken(user.Id, user.UserName, roles);
            var accessExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpires,
                RefreshToken = plaintextNewRefreshToken, //_refreshTokenSettings.UseCookie ? null : plaintextNewRefreshToken,
                RefreshTokenExpiresAt = newRefreshTokenEntity.ExpiresAt
            };
        }

        // --------------------------------------------------------------------
        // 🔹 LOGOUT — revoke đúng refresh token của phiên hiện tại (thiết bị khác không bị ảnh hưởng)
        // --------------------------------------------------------------------
        public async Task LogoutAsync(string refreshToken, string ip, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;

            var hashedToken = JwtTokenHelper.HashToken(refreshToken);
            var refreshTokenExisting = await _unitOfWork.Repository<RefreshToken>().Query()
                .FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken, ct);
            if (refreshTokenExisting == null || refreshTokenExisting.RevokedAt != null)
            {
                // Token không tồn tại hoặc đã bị revoke → không làm gì cả (idempotent)
                return;
            }
            // Revoke token hiện tại
            refreshTokenExisting.RevokedAt = DateTime.UtcNow;
            refreshTokenExisting.RevokedByIp = ip;
            refreshTokenExisting.ReasonRevoked = "Người dùng logout";
            _unitOfWork.Repository<RefreshToken>().Update(refreshTokenExisting);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // --------------------------------------------------------------------
        // 🔹 LOGOUT ALL — thu hồi toàn bộ refresh token đang hoạt động của user (mọi thiết bị)
        // --------------------------------------------------------------------
        public async Task LogoutAllAsync(string username, string ip, CancellationToken ct)
        {
            var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.UserName == username, ct);

            if(user is null)
            {
                return;
            }

            await RevokeAllActiveTokensAsync(user.Id, ip, "Người dùng logout khỏi mọi thiết bị", ct);
        }

        // --------------------------------------------------------------------
        // 🔹 CURRENT USER
        // --------------------------------------------------------------------

        public async Task<CurrentUser?> CurrentUserAsync(string username)
        {
            var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.UserName == username);

            if (user is null)
            {
                return null;
            }

            return new CurrentUser
            {
                UserId = user.Id,
                Username = user.UserName,
                Fullname = user.FullName,
                Roles = [user.Role]
            };
        }

        // --------------------------------------------------------------------
        // 🔹 HELPERS
        // --------------------------------------------------------------------
        private RefreshToken BuildRefreshToken(int userId, string ip, out string plaintextToken)
        {
            plaintextToken = JwtTokenHelper.GenerateRefreshTokenPlaintext();

            return new RefreshToken
            {
                UserId = userId,
                TokenHash = JwtTokenHelper.HashToken(plaintextToken), // Lưu Hash của Refresh Token vào DB
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenSettings.ExpiryDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ip
            };
        }

        private async Task RevokeAllActiveTokensAsync(int userId, string ip, string reason, CancellationToken ct)
        {
            var activeTokens = await _unitOfWork.Repository<RefreshToken>().Query()
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);
            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ip;
                token.ReasonRevoked = reason;

                _unitOfWork.Repository<RefreshToken>().Update(token);
            }
            await _unitOfWork.SaveChangesAsync(ct);
        }
        
    }
}
