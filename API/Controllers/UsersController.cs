using API.Models.DTOs;
using API.Models.Entities;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var users = await _userService.GetAllUsersAsync(ct);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var user = await _userService.GetUserByIdAsync(id, ct);
            if (user == null)
                return NotFound("Không tìm thấy User.");
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserUpsertDto userDto, CancellationToken ct)
        {
            await _userService.CreateAsync(userDto, ct);
            return CreatedAtAction(nameof(GetById), new { id = userDto.Id }, userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpsertDto userDto, CancellationToken ct)
        {
            if (id != userDto.Id)
            {
                return BadRequest("ID không khớp");
            }
            await _userService.UpdateAsync(userDto, ct);
            return NoContent();
        }
    }
}
