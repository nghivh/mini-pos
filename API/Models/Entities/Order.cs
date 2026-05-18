using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public int? CustomerId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
