namespace API.Models.DTOs
{
    /// <summary>
    /// Dto dùng để tạo mới hoặc cập nhật sản phẩm
    /// </summary>
    public class ProductUpsertDto
    {
        public int? Id { get; set; }
        public int CategoryId { get; set; }
        public string? Barcode { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Dto dùng để trả về thông tin sản phẩm, bao gồm cả tên danh mục
    /// </summary>
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? Barcode { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Dto dùng để nhận các tham số truy vấn khi lấy danh sách sản phẩm, hỗ trợ tìm kiếm, lọc và phân trang
    /// </summary>
    public class ProductQueryRequest
    {
        public string? Search { get; set; }     // Tìm theo tên sản phẩm hoặc mã vạch
        public int? CategoryId { get; set; }    // Tìm theo danh mục (null = tất cả)
        public decimal? MinPrice { get; set; }  
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }     // Sắp xếp theo cột nào (ProductName, Price...)
        public bool IsDescending { get; set; }  // Sắp xếp giảm dần?
        public int Page { get; set; } = 1;      // Trang hiện tại
        public int PageSize { get; set; } = 10; // Số bản ghi trên mỗi trang
    }
}
