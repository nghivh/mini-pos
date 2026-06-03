using API.Models.DTOs;

namespace API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(CheckoutRequest request, CancellationToken ct);
    }
}
