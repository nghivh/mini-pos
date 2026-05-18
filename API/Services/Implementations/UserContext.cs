using API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace API.Services.Implementations
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;

        private string _ipAddress;
        private string _machineName;

        public UserContext(IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public string IpAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(_ipAddress)) return _ipAddress;
                _ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";
                return _ipAddress;
            }
        }

        public string MachineName
        {
            get
            {
                // 1. Nếu đã có trong biến cục bộ (scope request) -> Trả về ngay
                if (!string.IsNullOrEmpty(_machineName)) return _machineName;

                // 2. Nếu chưa có IP -> Không làm gì được
                if (string.IsNullOrEmpty(IpAddress)) return "Unknown";

                // 3. KEY để lưu cache: Ví dụ "DNS_192.168.1.50"
                string cacheKey = $"DNS_{IpAddress}";

                // 4. Kiểm tra trong Cache hệ thống (Memory) xem đã resolve IP này chưa?
                if (!_cache.TryGetValue(cacheKey, out string cachedName))
                {
                    // A. Nếu chưa có trong Cache -> Phải đi hỏi DNS (Mất thời gian ở đây)
                    try
                    {
                        cachedName = Dns.GetHostEntry(IpAddress).HostName;
                    }
                    catch
                    {
                        cachedName = "Unknown";
                    }

                    // B. Lưu kết quả vào Cache trong 60 phút
                    // Lần sau user này (hoặc user khác cùng IP này) vào sẽ lấy từ đây, không cần resolve nữa.
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

                    _cache.Set(cacheKey, cachedName, cacheOptions);
                }

                // 5. Gán kết quả
                _machineName = cachedName;
                return _machineName;
            }
        }
    }
}
