using API.Application.Interfaces;
using API.Common.Security;
using API.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly RefreshTokenSettings _refreshTokenSettings;

        public AuthController(IAuthService authService, IOptions<RefreshTokenSettings> refreshTokenSettings)
        {
            _authService = authService;
            _refreshTokenSettings = refreshTokenSettings.Value;
        }

        // LOGIN
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest rq, CancellationToken ct)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var device = Request.Headers["User-Agent"].ToString();

            var result = await _authService.LoginAsync(rq.Username, rq.Password, device, ip, ct);

            // Nếu dùng cookie để lưu refresh token, thì set cookie ở đây
            if(_refreshTokenSettings.UseCookie && !string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
                result.RefreshToken = null; // Không trả về body
            }

            return Ok(result);
        }

        // REFRESH TOKEN
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest rq, CancellationToken ct)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var device = Request.Headers["User-Agent"].ToString();

            // Lấy plaintext từ cookie hoặc body (tùy config)
            var tokenPlain = _refreshTokenSettings.UseCookie ? Request.Cookies[_refreshTokenSettings.CookieName] : rq.RefreshToken;

            if (string.IsNullOrWhiteSpace(tokenPlain))
                return Unauthorized(new { message = "Missing refresh token" });

            var result = await _authService.RefreshAsync(tokenPlain, rq.ExpiredAccessToken);

            if (_refreshTokenSettings.UseCookie && !string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
                result.RefreshToken = null;
            }

            return Ok(result);
        }

        // CURRENT USER INFO
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var currentUser = await _authService.CurrentUserAsync(username);

            return Ok(currentUser);
        }

        // Helper method: Set cookie
        private void SetRefreshCookie(string tokenPlain, DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = _refreshTokenSettings.CookieSecure,
                SameSite = _refreshTokenSettings.CookieSameSiteStrict ? SameSiteMode.Strict : SameSiteMode.Lax,
                Expires = expiresAt
            };
            Response.Cookies.Append(_refreshTokenSettings.CookieName, tokenPlain, cookieOptions);
        }
    }
}
