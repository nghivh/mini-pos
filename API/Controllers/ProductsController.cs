using API.Models.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            this._productService = productService;
        }

        [HttpPost("getall")]
        public async Task<IActionResult> GetAll([FromBody] ProductQueryRequest queryRequest, CancellationToken ct)
        {
            var products = await _productService.GetAllAsync(queryRequest, ct);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var product = await _productService.GetByIdAsync(id, ct);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("barcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode, CancellationToken ct)
        {
            var product = await _productService.GetByBarcodeAsync(barcode, ct);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductUpsertDto productDto, CancellationToken ct)
        {
            var product = await _productService.CreateAsync(productDto, ct);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpsertDto productDto, CancellationToken ct)
        {
            var updatedProduct = await _productService.UpdateAsync(id, productDto, ct);
            if (updatedProduct == null)
            {
                return NotFound();
            }
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _productService.DeleteAsync(id, ct);
            if (!result)
            {
                return NotFound();
            }

            // Theo chuẩn REST, xóa thành công thường trả về No Content
            return NoContent();
        }
    }
}
