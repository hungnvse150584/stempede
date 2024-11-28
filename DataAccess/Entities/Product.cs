using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(255)]
    public string ProductName { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    [MaxLength(10)]
    public string? Ages { get; set; }
    public string? ImagePath { get; set; }

    [Required]
    public int SupportInstances { get; set; }

    [ForeignKey("Lab")]
    public int LabId { get; set; }

    [ForeignKey("Subcategory")]
    public int SubcategoryId { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Lab Lab { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Subcategory Subcategory { get; set; } = null!;
}
