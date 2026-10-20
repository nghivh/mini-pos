using API.Common.Security;
using API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace API.Services.Implementations
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;

        public UserContext(IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        private ClaimsPrincipal? CurrentPrincipal => _httpContextAccessor.HttpContext?.User;

        public int? UserId
        {
            get 
            {
                var value = CurrentPrincipal?.FindFirstValue(JwtTokenHelper.UserIdClaimType);
                return int.TryParse(value, out var id) ? id : null;
            }
        }

        public string? UserName
        {
            get
            {
                var value = CurrentPrincipal?.FindFirstValue(ClaimTypes.Name) 
                    ?? CurrentPrincipal?.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
                return value;
            }
        }

        public string IpAddress
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if(context == null) return string.Empty;

                // Xử lý trường hợp ứng dụng đứng sau Proxy/Load Balancer (Nginx, Cloudflare, v.v.)
                if(context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
                {
                    var ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
                    if(!string.IsNullOrEmpty(ip)) return ip;
                }

                return context.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
            }
        }

        public string MachineName
        {
            get
            {
                var ip = IpAddress;
                if(string.IsNullOrEmpty(ip) || ip == "::1" || ip == "127.0.0.1")
                {
                    return Environment.MachineName; // Hoặc trả về Localhost nếu là chạy dưới máy dev
                }

                string cacheKey = $"DNS_{ip}";

                return _cache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30); // Cache 30 phút
                    try
                    {
                        var hostEntry = Dns.GetHostEntry(ip);
                        return hostEntry.HostName;
                    }
                    catch
                    {
                        return "Unknown";
                    }
                }) ?? "Unknown";
            }
        }

        public string Device
        {
            get
            {
                var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
                return !string.IsNullOrEmpty(userAgent) ? userAgent : "Unknown";
            }
        }
    }
}
