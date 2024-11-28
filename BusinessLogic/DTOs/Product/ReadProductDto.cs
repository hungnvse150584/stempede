namespace BusinessLogic.DTOs.Product
{
    public class ReadProductDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? Ages { get; set; }

        public int SupportInstances { get; set; }

        public int LabId { get; set; }

        public string LabName { get; set; } = null!;

        public int SubcategoryId { get; set; }

        public string SubcategoryName { get; set; } = null!;
    }
}
