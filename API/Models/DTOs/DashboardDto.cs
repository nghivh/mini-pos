namespace API.Models.DTOs
{
    public class DashboardSummaryResponse    
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalProductsSold { get; set; }

        // Các chỉ số tăng trưởng (%)
        public double RevenueGrowth { get; set; }
        public double OrdersGrowth { get; set; }
        public double AvgValueGrowth { get; set; }
        public double ProductsSoldGrowth { get; set; }

        public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
        public List<RecentOrderDto> RecentOrders { get; set; } = new List<RecentOrderDto>();
        public List<ChartDataDto> ChartData { get; set; } = new List<ChartDataDto>();
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? CustomerName { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ChartDataDto
    {
        public string Label { get; set; }       // Nhãn trục X (Giờ hoặc Ngày)
        public decimal Revenue { get; set; }    // Tổng tiền ở mốc thời gian đó
    }
}
