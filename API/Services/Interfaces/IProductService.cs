using API.Common.Helpers;
using API.Models.DTOs;

namespace API.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductResponseDto>> GetAllAsync(ProductQueryRequest queryRequest, CancellationToken ct);
        Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<ProductResponseDto?> GetByBarcodeAsync(string barcode, CancellationToken ct);

        Task<ProductResponseDto> CreateAsync(ProductUpsertDto productDto, CancellationToken ct);
        Task<ProductResponseDto> UpdateAsync(int id, ProductUpsertDto productDto, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
