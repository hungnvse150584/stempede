using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities;

public partial class Subcategory
{
    [Key]
    public int SubcategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SubcategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
