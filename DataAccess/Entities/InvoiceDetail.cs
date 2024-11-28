using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class InvoiceDetail
{
    public int InvoiceDetailId { get; set; }

    public int? InvoiceId { get; set; }

    public string? ProductName { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public virtual Invoice? Invoice { get; set; }
}
