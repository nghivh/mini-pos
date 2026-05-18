using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Common.Security
{
    public class JwtTokenHelper
    {
        private readonly JwtSettings _jwtSettings;
        public JwtTokenHelper(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public string GenerateAccessToken(string username, IEnumerable<string> roles)
        {
            // Tạo key bảo mật từ chuỗi SecretKey
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            // Dùng thuật toán HMAC SHA256 để ký token
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Gắn thông tin người dùng vào claims
            var claims = new List<Claim>
            {
                // JwtRegisteredClaimNames: Chuẩn JWT / tương tác API, client, hệ thống khác
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Thêm các vai trò (roles) vào claims
            foreach (var role in roles)
            {
                // ClaimTypes: Tích hợp ASP.NET Identity, [Authorize(Roles = "...")]
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateRefreshTokenPlaintext(int size = 64) 
        {
            // Tạo chuỗi ngẫu nhiên làm refresh token
            var bytes = RandomNumberGenerator.GetBytes(size);
            return Convert.ToBase64String(bytes);
        }

        public static string HashToken(string token)
        {
            // Băm (hash) refresh token trước khi lưu vào CSDL
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        // Hàm tạo Refresh Token dạng JWT (đơn giản, chỉ chứa username)
        public string GenerateSimpleRefreshTokenJwt(string username, DateTime expiresAt)
        {
            // Tạo key bảo mật từ chuỗi SecretKey
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            // Dùng thuật toán HMAC SHA256 để ký token
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Gắn thông tin người dùng vào claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username), // Lưu username để biết của ai
                new Claim("token_type", "refresh_token")          // Đánh dấu đây là refresh token
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt, // Thời gian hết hạn dài (ví dụ 7 ngày)
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Hàm Validate Refresh Token (Kiểm tra xem token có hợp lệ và CÒN HẠN không)
        public ClaimsPrincipal? ValidateRefreshToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    // 1. Kiểm tra chữ ký (Quan trọng nhất: để đảm bảo token do server cấp)
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    // 2. Kiểm tra Issuer & Audience (Tùy config của bạn, nên để true nếu có)
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,

                    // 3. Kiểm tra thời hạn (BẮT BUỘC: Refresh Token phải còn hạn mới dùng được)
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Loại bỏ độ lệch thời gian mặc định (5 phút) cho chính xác
                };

                // Thực hiện validate
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Kiểm tra kỹ thuật toán mã hóa (Chống tấn công 'none' algorithm)
                if (validatedToken is JwtSecurityToken jwtSecurityToken &&
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                // Nếu token hết hạn, sai chữ ký, hoặc lỗi format -> Trả về null
                return null;
            }
        }

        // Hàm lấy thông tin từ Access Token đã hết hạn (để lấy lại Roles cũ)
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = false // QUAN TRỌNG: Cho phép token đã hết hạn
                };

                // Thực hiện validate
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                // Kiểm tra kỹ thuật toán mã hóa (Chống tấn công 'none' algorithm)
                if (securityToken is JwtSecurityToken jwtSecurityToken &&
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
