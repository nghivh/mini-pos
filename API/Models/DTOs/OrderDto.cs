namespace API.Models.DTOs
{
    // 1. Chi tiết từng món trong giỏ hàng gửi lên
    public class OrderDetailDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // 2. Yêu cầu thanh toán tổng thể
    public class CheckoutRequest
    {
        public int? CustomerId { get; set; }
        public decimal DiscountAmount { get; set; }      
        public string? PaymentMethod { get; set; } // Cash, Bank, Momo
        public string? Notes { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; } = new List<OrderDetailDto>();
    }
}
