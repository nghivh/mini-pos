using API.Models.Entities;

namespace API.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync(CancellationToken ct);
        Task<Customer?> GetCustomerByPhoneAsync(string phoneNumber, CancellationToken ct);
        Task CreateAsync(Customer customer, CancellationToken ct);
        Task UpdateAsync(Customer customer, CancellationToken ct);
    }
}
