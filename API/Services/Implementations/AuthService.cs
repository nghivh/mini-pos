using API.Application.Interfaces;
using API.Common.Security;
using API.Data.Interfaces;
using API.Models.DTOs.Auth;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly JwtSettings _jwtSettings;
        private readonly RefreshTokenSettings _refreshTokenSettings;

        public AuthService(IUnitOfWork unitOfWork, JwtTokenHelper jwtTokenHelper, IOptions<JwtSettings> jwtSettings, IOptions<RefreshTokenSettings> refreshTokenSettingsOptions)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenHelper = jwtTokenHelper;
            _refreshTokenSettings = refreshTokenSettingsOptions.Value;
            _jwtSettings = jwtSettings.Value;
        }

        // --------------------------------------------------------------------
        // 🔹 LOGIN
        // --------------------------------------------------------------------
        public async Task<AuthResponse> LoginAsync(string username, string password, string? device, string ip, CancellationToken ct)
        {
            // 1️. Lấy user + roles
            /*
            string encodedPassword = ComputeHash(password);
            */

            //For test mini app
            if (username != "admin" || password != "123456")
            {
                throw new UnauthorizedAccessException("User hoặc mật khẩu không đúng");
            }
            string[] roleNames = ["admin"];
            //For test mini app

            // 3️. Tạo Access Token
            var accessToken = _jwtTokenHelper.GenerateAccessToken(username, roleNames);
            var accessExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            // 4️. Tạo Refresh Token
            var refreshExpires = DateTime.UtcNow.AddDays(_refreshTokenSettings.ExpiryDays);
            // Hàm này sẽ tạo ra một chuỗi JWT dài có chứa username và thời hạn
            var refreshTokenJwt = _jwtTokenHelper.GenerateSimpleRefreshTokenJwt(username, refreshExpires);

            // 5️. Trả kết quả
            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpires,
                RefreshToken = _refreshTokenSettings.UseCookie ? null : refreshTokenJwt,
                RefreshTokenExpiresAt = refreshExpires
            };
        }

        // --------------------------------------------------------------------
        // 🔹 REFRESH TOKEN
        // --------------------------------------------------------------------
        public async Task<AuthResponse> RefreshAsync(string refreshToken, string expiredAccessToken)
        {
            // 1. Validate Refresh Token
            // Hàm này kiểm tra chữ ký và hạn sử dụng (Expire)
            var refreshPrincipal = _jwtTokenHelper.ValidateRefreshToken(refreshToken);

            if (refreshPrincipal is null)
                throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");

            // Lấy username từ Refresh Token
            var username = refreshPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? refreshPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(username))
                throw new UnauthorizedAccessException("Token lỗi.");

            // 2. Lấy Roles từ Access Token Cũ
            var accessPrincipal = _jwtTokenHelper.GetPrincipalFromExpiredToken(expiredAccessToken);
            var oldRoles = accessPrincipal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                           ?? accessPrincipal?.FindAll("role").Select(c => c.Value).ToList();

            if (oldRoles == null) oldRoles = new List<string>();

            // 3. Cấp Token Mới
            var newAccessToken = _jwtTokenHelper.GenerateAccessToken(username, oldRoles);
            var newAccessExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            // Xoay vòng Refresh Token (Tạo cái mới tiếp)
            var newRefreshExpires = DateTime.UtcNow.AddDays(_refreshTokenSettings.ExpiryDays);
            var newRefreshToken = _jwtTokenHelper.GenerateSimpleRefreshTokenJwt(username, newRefreshExpires);

            var response = new AuthResponse
            {
                AccessToken = newAccessToken,
                AccessTokenExpiresAt = newAccessExpires,
                RefreshToken = _refreshTokenSettings.UseCookie ? null : newRefreshToken,
                RefreshTokenExpiresAt = newRefreshExpires
            };

            return await Task.FromResult(response);
        }

        // --------------------------------------------------------------------
        // 🔹 CURRENT USER
        // --------------------------------------------------------------------

        public async Task<CurrentUser?> CurrentUserAsync(string username)
        {
            //For test mini app
            if(username == "admin")
            {
                return new CurrentUser
                {
                    Username = username,
                    Fullname = "Admin",
                    Section = "Admin",
                    Roles = []
                };
            }

            var param = new
            {
                EmpCode = username
            };

            var currentUser = await _unitOfWork.Dapper.ExecStoredProcToDataTableAsync("APPDB.dbo.p_sys_get_user_info", param);

            if(currentUser is null || currentUser.Rows.Count <= 0)
            {
                return null;
            }

            return new CurrentUser
            {
                Username = username,
                Fullname = currentUser.Rows[0]["FullName"].ToString()!,
                Section = currentUser.Rows[0]["SectionCode"].ToString()!,
                Roles = []
            };
        }

        private string ComputeHash(string text)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    stringBuilder.Append(hash[i].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }        
    }
}
