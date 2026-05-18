using API.Models.Entities;

namespace API.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken ct);
        Task CreateAsync(Category category, CancellationToken ct);
        Task UpdateAsync(Category category, CancellationToken ct);
    }
}
