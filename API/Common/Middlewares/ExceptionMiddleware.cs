using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;

namespace API.Common.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                var statusCode = ex switch
                {
                    // --- NHÓM 1: Lỗi Client (400) ---
                    ValidationException => HttpStatusCode.BadRequest,
                    ArgumentNullException => HttpStatusCode.BadRequest,
                    ArgumentException => HttpStatusCode.BadRequest,
                    FormatException => HttpStatusCode.BadRequest,
                    BadHttpRequestException => HttpStatusCode.BadRequest,

                    // --- NHÓM 2: Bảo mật (401/403) ---
                    AuthenticationException => HttpStatusCode.Unauthorized,
                    UnauthorizedAccessException => HttpStatusCode.Forbidden,

                    // --- NHÓM 3: Tài nguyên (404/409) ---
                    KeyNotFoundException => HttpStatusCode.NotFound,
                    DbUpdateConcurrencyException => HttpStatusCode.Conflict,
                    DbUpdateException => HttpStatusCode.Conflict,

                    // --- NHÓM 4: Hệ thống/Hủy ---
                    OperationCanceledException => (HttpStatusCode)499,
                    NotImplementedException => HttpStatusCode.NotImplemented,

                    // 👇 Bổ sung lại lỗi SQL (Mất kết nối, timeout...)
                    // SqlException => HttpStatusCode.InternalServerError, 

                    // --- Mặc định ---
                    _ => HttpStatusCode.InternalServerError
                };

                response.StatusCode = (int)statusCode;

                // ---------------------------------------------------------
                // 👇 LOGIC SỬA MESSAGE
                // ---------------------------------------------------------
                var message = ex.Message; // 1. Lấy message gốc

                if (ex is DbUpdateException)
                {
                    message = "Lỗi cập nhật dữ liệu. Có thể do trùng lặp hoặc ràng buộc dữ liệu."; // 2. Custom lại
                }

                var errorResponse = new ApiException(
                    statusCode: (int)statusCode,
                    message: message, // 👈 [QUAN TRỌNG] Phải dùng biến 'message' đã custom, đừng dùng 'ex.Message'
                    details: _env.IsDevelopment() ? ex.StackTrace?.ToString() : null
                );

                // Log error
                if (ex is OperationCanceledException)
                {
                    _logger.LogWarning("Request was cancelled by client.");
                }
                else if ((int)statusCode >= 500)
                {
                    _logger.LogError(ex, "[Server Error] {Message}", ex.Message);
                }
                else
                {
                    _logger.LogWarning("[Client Error] {Code} - {Message}", statusCode, ex.Message);
                }

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(errorResponse, options);
                await response.WriteAsync(json);
            }
        }
    }

    public class ApiException
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Details { get; set; }

        public ApiException(int statusCode, string message, string? details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }
    }
}
