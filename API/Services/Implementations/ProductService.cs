using API.Common.Helpers;
using API.Data.Extensions;
using API.Data.Interfaces;
using API.Models.DTOs;
using API.Models.Entities;
using API.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<ProductResponseDto>> GetAllAsync(ProductQueryRequest queryRequest, CancellationToken ct)
        {
            // 1. Khởi tạo query từ Repository (mặc định NoTracking để tối ưu hiệu năng đọc)
            var query = _unitOfWork.Repository<Product>()
                .Query(asNoTracking: true)
                .Include(x => x.Category) // Include Category để lấy tên danh mục
                .AsQueryable();

            // 2. Sử dụng bộ Extensions để build query động
            query = query
                // Tìm kiếm toàn văn (LIKE %keyword%) trên Barcode và ProductName
                .ApplySearch(queryRequest.Search, x => x.ProductName, x => x.Barcode)

                // So sánh bằng cho CategoryId (tự bỏ qua nếu queryRequest.CategoryId null)
                .ApplyEquals(x => x.CategoryId, queryRequest.CategoryId)

                // Lọc khoảng giá [MinPrice, MaxPrice] (tự bỏ qua các đầu null)
                .ApplyBetween(x => x.Price, queryRequest.MinPrice, queryRequest.MaxPrice)

                // Lọc những sản phẩm active
                .ApplyEquals(x => x.IsActive, true);

            // Lọc trạng thái hoạt động (nếu cần)
            //.WhereIf(queryRequest.IsActive.HasValue, p => p.IsActive == queryRequest.IsActive.Value)

            // 3. Sắp xếp động (sortBy là tên cột dưới dạng string, mặc định ID giảm dần)
            //query = query.ApplySorting(queryRequest.SortBy, queryRequest.IsDescending, defaultSort: "Id");
            if (!string.IsNullOrEmpty(queryRequest.SortBy))
            {
                // Upercase ký tự đầu tiên để Client trùng với API. (VD: productName => ProductName)
                var sortBy = queryRequest.SortBy;
                sortBy = char.ToUpper(sortBy[0]) + sortBy.Substring(1);
                query = query.ApplySorting(sortBy, queryRequest.IsDescending, defaultSort: "Id");
            }

            // 4. Thực thi phân trang và lấy tổng bản ghi bằng ToPagedListAsync của bạn
            // Lưu ý: Kết quả trả về từ Extension là (List<Product> Items, long Total)
            var (items, total) = await query.ToPagedListAsync(queryRequest.Page, queryRequest.PageSize, ct);

            // 5. Map kết quả sang DTO bằng AutoMapper
            var itemsDto = _mapper.Map<List<ProductResponseDto>>(items);

            return new PagedResult<ProductResponseDto>
            {
                Items = itemsDto,
                TotalCount = total,
                Page = queryRequest.Page,
                PageSize = queryRequest.PageSize
            };
        }

        public async Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            var product = await _unitOfWork.Repository<Product>()
                .Query(asNoTracking: true)                
                .Include(x => x.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto?> GetByBarcodeAsync(string barcode, CancellationToken ct)
        {
            var product = await _unitOfWork.Repository<Product>()
                .Query(asNoTracking: true)
                .Where(p => p.Barcode == barcode && p.IsActive)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(ct);

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto> CreateAsync(ProductUpsertDto productDto, CancellationToken ct)
        {
            // Kiểm tra mã vạch đã tồn tại chưa
            var existingProduct = await _unitOfWork.Repository<Product>()
                .Query(asNoTracking: true)
                .Where(p => p.Barcode == productDto.Barcode)
                .FirstOrDefaultAsync(ct);
            if (existingProduct != null)
            {
                throw new InvalidOperationException("Barcode sản phẩm này đã tồn tại.");
            }

            //  Map từ DTO sang Entity
            var product = _mapper.Map<Product>(productDto);

            // Thêm sản phẩm mới vào database
            await _unitOfWork.Repository<Product>().AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Lấy ra dữ liệu đầy đủ
            var result = await _unitOfWork.Repository<Product>()
                .Query(asNoTracking: true)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id, ct);

            // Map lại từ Entity sang DTO để trả về response
            return _mapper.Map<ProductResponseDto>(result);
        }

        public async Task<ProductResponseDto> UpdateAsync(int id, ProductUpsertDto productDto, CancellationToken ct)
        {
            // 1. Lấy Entity từ DB (Tracking để AutoMapper map đè lên)
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, ct);
            if (product == null || !product.IsActive) { throw new InvalidOperationException("Sản phẩm không tồn tại hoặc đã bị vô hiệu hóa."); }

            // 2. Kiểm tra mã vạch nếu có thay đổi (tránh trùng với sản phẩm khác)
            if (!string.IsNullOrEmpty(productDto.Barcode) && productDto.Barcode != product.Barcode)
            {
                var existingProduct = await _unitOfWork.Repository<Product>()
                    .Query(asNoTracking: true)
                    .Where(p => p.Barcode == productDto.Barcode && p.Id != id)
                    .FirstOrDefaultAsync(ct);
                if (existingProduct != null)
                {
                    throw new InvalidOperationException("Barcode sản phẩm này đã tồn tại ở sản phẩm khác.");
                }
            }

            // 3. Sử dụng AutoMapper để map đè dữ liệu từ DTO vào Entity
            _mapper.Map(productDto, product);

            // 4. Cập nhật Entity trong database
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            // 5. Lấy lại dữ liệu đầy đủ sau khi cập nhật (nếu cần)
            var result = await _unitOfWork.Repository<Product>()
                .Query(asNoTracking: true)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id, ct);
            
            return _mapper.Map<ProductResponseDto>(result);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, ct);
            if (product == null || !product.IsActive) { throw new InvalidOperationException("Sản phẩm không tồn tại hoặc đã bị vô hiệu hóa."); }

            product.IsActive = false;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return true;

            // Nếu muốn xóa luôn thì dùng đoạn code sau:
            /*
            _unitOfWork.Repository<Product>().Delete(product);
            var affectedRows = await _unitOfWork.SaveChangesAsync(ct);
            return affectedRows > 0;
            */
        }
    }
}
