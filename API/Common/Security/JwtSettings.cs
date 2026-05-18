namespace API.Common.Security
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryInMinutes { get; set; }
        public int ClockSkewInMinutes { get; set; } = 0;
    }

    public class RefreshTokenSettings
    {
        public int ExpiryDays { get; set; } = 7;                // Refresh TTL
        public bool UseCookie { get; set; }                     // gửi cookie HttpOnly
        public string CookieName { get; set; } = "rt";
        public bool CookieSecure { get; set; } = true;
        public bool CookieSameSiteStrict { get; set; } = true;
    }
}
