using API.Data.Interfaces;
using API.Models.Entities;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken ct)
        {
            return await _unitOfWork.Repository<Category>().Query().ToListAsync(ct);
        }

        public async Task CreateAsync(Category category, CancellationToken ct)
        {
            var existingCategory = await _unitOfWork.Repository<Category>().Query().FirstOrDefaultAsync(c => c.Name == category.Name, ct);
            if (existingCategory != null)
            {
                throw new ValidationException($"Danh mục với tên {category.Name} đã tồn tại.");
            }

            await _unitOfWork.Repository<Category>().AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Category category, CancellationToken ct)
        {
            var exsitingCategory = await _unitOfWork.Repository<Category>().Query(asNoTracking: true).FirstOrDefaultAsync(c => c.Id == category.Id, ct);
            if(exsitingCategory == null)
            {
                throw new ValidationException($"Không tồn tại Mã danh mục {category.Id}.");
            }

            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
