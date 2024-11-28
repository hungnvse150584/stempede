using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities;

public partial class Lab
{
    [Key]
    public int LabId { get; set; }

    [MaxLength(255)]
    public string? LabName { get; set; }

    public string? Description { get; set; }

    [MaxLength(255)]
    public string? LabFileUrl { get; set; }

    // Navigation Property
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
