using API.Models.Entities;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var customers = await _customerService.GetAllCustomersAsync(ct);
            return Ok(customers);
        }

        [HttpGet("by-phone/{phoneNumber}")]
        public async Task<IActionResult> GetByPhone(string phoneNumber, CancellationToken ct)
        {
            var customer = await _customerService.GetCustomerByPhoneAsync(phoneNumber, ct);
            if (customer == null)
                return NotFound("Không tìm thấy Khách hàng.");
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Customer customer, CancellationToken ct)
        {
            await _customerService.CreateAsync(customer, ct);
            return CreatedAtAction(nameof(GetByPhone), new { phoneNumber = customer.PhoneNumber }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Customer customer, CancellationToken ct)
        {
            if (id != customer.Id)
            {
                return BadRequest("ID không khớp");
            }

            await _customerService.UpdateAsync(customer, ct);
            return NoContent();
        }

    }
}
