using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Common.Security
{
    public static class JwtConfiguration
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Đọc cấu hình từ appsettings.json
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // Đăng ký helper
            services.AddSingleton<JwtTokenHelper>();

            // Lấy thông tin JwtSettings
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            // Thêm Authentication dùng JWT Bearer
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Cấu hình xác thực Token
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });

            return services;
        }
    }
}
