using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Refund
{
    public int RefundId { get; set; }

    public int? InvoiceId { get; set; }

    public decimal RefundAmount { get; set; }

    public DateTime RefundDate { get; set; }

    public string RefundReason { get; set; } = null!;

    public virtual Invoice? Invoice { get; set; }
}
