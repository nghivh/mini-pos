using API.Data.Interfaces;
using API.Models.DTOs;
using API.Models.Entities;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace API.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            // Đảm bảo endDate luôn kết thúc ở cuối ngày
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            // Tính toán khoảng thời gian của kỳ trước
            TimeSpan period = endDate - startDate;
            DateTime previousStartDate = startDate - period;
            DateTime previousEndDate = startDate.AddTicks(-1);

            // Lấy các đơn hàng trong kỳ hiện tại
            var orders = await _unitOfWork.Repository<Order>().Query()
                .Include(c => c.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync(ct);

            // Lấy các đơn hàng trong kỳ trước
            var previousOrders = await _unitOfWork.Repository<Order>().Query()
                .Include(c => c.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate >= previousStartDate && o.OrderDate <= previousEndDate)
                .ToListAsync(ct);

            var response = new DashboardSummaryResponse();

            // 1. Tính toán Quick Stats kỳ hiện tại
            response.TotalOrders = orders.Count;
            response.TotalRevenue = orders.Sum(o => o.FinalAmount);
            response.AverageOrderValue = response.TotalOrders > 0 ? response.TotalRevenue / response.TotalOrders : 0;
            response.TotalProductsSold = orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity));

            // Tính toán Quick Stats kỳ trước
            int previousTotalOrders = previousOrders.Count;
            decimal previousTotalRevenue = previousOrders.Sum(o => o.FinalAmount);
            decimal previousAverageOrderValue = previousTotalOrders > 0 ? previousTotalRevenue / previousTotalOrders : 0;
            int previousTotalProductsSold = previousOrders.Sum(o => o.OrderDetails.Sum(od => od.Quantity));

            // 2. Lấy Top Sản phẩm bán chạy (Gom nhóm theo ProductId)
            response.TopProducts = orders.SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.ProductId)
                .Select(g => new TopProductDto
                {
                    ProductName = g.First().Product.ProductName,
                    QuantitySold = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(tp => tp.QuantitySold)
                .Take(5)
                .ToList();

            // 3. Lấy 10 Giao dịch gần đây
            response.RecentOrders = orders.OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    CustomerName = o.CustomerId != null ? $"{o.Customer?.FullName} ({o.Customer?.PhoneNumber})" : "Khách lẻ",
                    OrderDate = o.OrderDate,
                    PaymentMethod = o.PaymentMethod!,
                    TotalAmount = o.FinalAmount,
                    Status = "Hoàn thành"
                })
                .ToList();

            // 4. Tính toán phần trăm tăng trưởng so với kỳ trước
            response.RevenueGrowth = CalculateGrowth(response.TotalRevenue, previousTotalRevenue);
            response.OrdersGrowth = CalculateGrowth(response.TotalOrders, previousTotalOrders);
            response.AvgValueGrowth = CalculateGrowth(response.AverageOrderValue, previousAverageOrderValue);
            response.ProductsSoldGrowth = CalculateGrowth(response.TotalProductsSold, previousTotalProductsSold);

            // 5. Xử lý dữ liệu biểu đồ
            TimeSpan duration = endDate - startDate;
            var chartData = new List<ChartDataDto>();

            // Trường hợp 1: Khoảng thời gian lọc <= 1 ngày (VD: Hôm nay)
            if(duration.TotalDays <= 1)
            {
                // Duyệt qua 24 khung giờ (từ 0h đến 23h)
                for(int i = 0; i <= 23; i++)
                {
                    // Tính tổng tiền các đơn có giờ (Hour) trùng khớp
                    decimal revenueForHour = orders
                        .Where(o => o.OrderDate.Hour == i)
                        .Sum(o => o.TotalAmount);

                    chartData.Add(new ChartDataDto
                    {
                        Label = $"{i:00}:00", // Format thành "08:00", "14:00"
                        Revenue = revenueForHour
                    });
                }
            }
            // Trường hợp 2: Khoảng thời gian lọc nhiều ngày (VD: Tuần, Tháng, Tùy chọn)
            else
            {
                // Duyệt qua từng ngày trong khoảng thời gian lọc
                for(var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    // Tính tổng tiền các đơn có ngày (Date) trùng khớp
                    decimal revenueForDay = orders
                        .Where(o => o.OrderDate.Date == date)
                        .Sum(o => o.TotalAmount);

                    chartData.Add(new ChartDataDto
                    {
                        Label = date.ToString("dd/MM"), // Format thành "01/06", "15/06"
                        Revenue = revenueForDay
                    });
                }
            }
            response.ChartData = chartData;

            return response;
        }

        // Hàm tiện ích để tính toán phần trăm tăng trưởng
        private double CalculateGrowth(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0) return currentValue > 0 ? 100 : 0; // Trường hợp giá trị trước là 0
            double growth = (double)((currentValue - previousValue) / previousValue) * 100;
            return Math.Round(growth, 1);
        }
    }
}
