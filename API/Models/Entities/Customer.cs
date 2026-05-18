using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities
{
    [Table("Customers")]
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = null!;

        public int LoyaltyPoints { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Quan hệ: Một khách hàng có thể có nhiều hóa đơn
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
