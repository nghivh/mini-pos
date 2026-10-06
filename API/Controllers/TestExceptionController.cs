#if DEBUG

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Cho phép truy cập mà không cần xác thực

    public class TestExceptionController : ControllerBase
    {
        [HttpGet("null-reference")]
        public IActionResult ThrowNullReference()
        {
            string? value = null;
            var length = value.Length; // gây NullReferenceException
            return Ok(length);
        }

        [HttpGet("unauthorized")]
        public IActionResult ThrowUnauthorized()
        {
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập tài nguyên này.");
        }

        [HttpGet("validation")]
        public IActionResult ThrowValidation()
        {
            throw new ValidationException("Dữ liệu không hợp lệ.");
        }

        //[HttpGet("sql-error")]
        //public IActionResult ThrowSqlError()
        //{
        //    throw new SqlException("Giả lập lỗi SQL", null); // dùng mock trong test thực tế
        //}

        [HttpGet("custom-error")]
        public IActionResult ThrowCustom()
        {
            throw new Exception("Lỗi không xác định!");
        }

        [HttpGet("not-found")]
        public IActionResult ThrowNotFound()
        {
            throw new KeyNotFoundException("Không tìm thấy dữ liệu!");
        }
    }
}

#endif