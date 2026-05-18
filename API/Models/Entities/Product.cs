using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; }

        [MaxLength(50)]
        public string? Barcode { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
    }
}
