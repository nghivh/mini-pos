namespace API.Models.DTOs.Auth
{
    /// <summary>
    /// Kết quả trả về sau khi đăng nhập hoặc làm mới token.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>Access token (JWT) dùng để gọi API.</summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>Thời điểm hết hạn của access token (UTC).</summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>Refresh token plaintext (nếu không dùng cookie).</summary>
        public string? RefreshToken { get; set; }

        /// <summary>Thời điểm hết hạn của refresh token (UTC).</summary>
        public DateTime RefreshTokenExpiresAt { get; set; }
    }

    /// <summary>
    /// Request đăng nhập người dùng.
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        /// <summary>Tên thiết bị (ví dụ: “Chrome”, “iPhone”, ...).</summary>
        public string? Device { get; set; }
    }

    /// <summary>
    /// Request làm mới token.
    /// </summary>
    public class RefreshTokenRequest
    {
        public string? RefreshToken { get; set; }
        public string ExpiredAccessToken { get; set; } = null!;        
    }

    /// <summary>
    /// Request thu hồi (revoke) token.
    /// </summary>
    public class RevokeTokenRequest
    {
        /// <summary>
        /// Nếu true → thu hồi tất cả token của user (logout all devices).
        /// Nếu false → chỉ logout thiết bị hiện tại.
        /// </summary>
        public bool RovokeAllSessions { get; set; } = false;
    }

    /// <summary>
    /// Kết quả trả về khi lấy thông tin đăng nhập.
    /// </summary>
    public class CurrentUser
    {
        public string Username { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string Section { get; set; } = null!;
        public string[] Roles { get; set; } = [];
    }
}
