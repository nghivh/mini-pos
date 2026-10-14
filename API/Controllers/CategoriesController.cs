using API.Models.Entities;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;
            var categories = await _categoryService.GetAllCategoriesAsync(CancellationToken.None);
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category, CancellationToken ct)
        {
            await _categoryService.CreateAsync(category, ct);
            return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category, CancellationToken ct)
        {
            if (id != category.Id)
            {
                return BadRequest("ID không khớp");
            }

            await _categoryService.UpdateAsync(category, ct);
            return NoContent();
        }
    }
}
