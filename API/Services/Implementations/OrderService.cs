using API.Data.Interfaces;
using API.Models.DTOs;
using API.Models.Entities;
using API.Services.Interfaces;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Runtime.CompilerServices;

namespace API.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateOrderAsync(CheckoutRequest request, CancellationToken ct)
        {
            // Sử dụng hàm ExecuteInTransactionAsync để đảm bảo tính toàn vẹn dữ liệu
            return await _unitOfWork.ExecuteInTransactionAsync(async (token) =>
            {
                // 1. Tính toán lại tổng tiền tại Server để đảm bảo an toàn
                decimal totalAmount = request.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
                decimal finalAmount = totalAmount - request.DiscountAmount;

                // 2. Chèn dữ liệu vào bảng Orders
                var insertOrder = new Order
                {
                    OrderDate = DateTime.UtcNow,
                    CustomerId = request.CustomerId,
                    TotalAmount = totalAmount,
                    DiscountAmount = request.DiscountAmount,
                    FinalAmount = finalAmount,
                    PaymentMethod = request.PaymentMethod,                  
                    Notes = request.Notes
                };
                await _unitOfWork.Repository<Order>().AddAsync(insertOrder, token);
                await _unitOfWork.SaveChangesAsync(token); // Lưu để lấy được OrderId

                // 3. Duyệt qua từng sản phẩm trong giỏ hàng
                foreach (var item in request.OrderDetails)
                {
                    #region "SQL Script"
                    /*
                    // A. Lưu chi tiết hóa đơn (OrderDetails)
                    string orderDetailInsertQuery = "p_Order_Insert_OrderDetail";
                    decimal subTotal = item.Quantity * item.UnitPrice;

                    await _unitOfWork.Dapper.ExecStoredProcAsync<int>(orderDetailInsertQuery, new
                    {
                        OrderId = orderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        SubTotal = subTotal
                    }, token);

                    // B. Trừ tồn kho (Quan trọng: Kiểm tra StockQuantity >= Quantity ngay tại câu SQL)
                    string stockUpdateQuery = @"
                        UPDATE Products
                        SET StockQuantity = StockQuantity - @Quantity
                        WHERE ProductId = @ProductId AND StockQuantity >= @Quantity;";

                    int effectedRows = await _unitOfWork.Dapper.ExecNonQueryAsync(stockUpdateQuery, new
                    {
                        ProductIdr = item.ProductId,
                        Quantity = item.Quantity    
                    }, token);

                    // C. Nếu affectedRows == 0 nghĩa là ID sai hoặc tồn kho không đủ
                    if(effectedRows == 0)
                    {
                        throw new Exception($"Không đủ tồn kho cho sản phẩm ID {item.ProductId} hoặc sản phẩm không tồn tại.");
                    }
                    */
                    #endregion

                    //Gói gọn lại thành một stored procedure
                    string insertOrderDetailQuery = "p_Process_Each_OrderDetail";
                    var parameters = new
                    {
                        OrderId = insertOrder.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        SubTotal = item.Quantity * item.UnitPrice
                    };
                    // Nếu hàm Raise error thì sẽ nhảy vào ExceptionMiddleWare -> Rollback Order
                    await _unitOfWork.Dapper.ExecStoredProcAsync<int>(insertOrderDetailQuery, parameters, token);
                }

                return insertOrder.Id;
            });
        }
    }
}
