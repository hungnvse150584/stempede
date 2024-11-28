using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required]
        [MaxLength(255)]
        public string ProductName { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity cannot be negative.")]
        public int StockQuantity { get; set; }

        [MaxLength(10)]
        public string? Ages { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Support Instances cannot be negative.")]
        public int SupportInstances { get; set; }

        [Required]
        public int LabId { get; set; }

        [Required]
        public int SubcategoryId { get; set; }
    }
}
