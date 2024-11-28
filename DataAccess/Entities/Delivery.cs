using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Delivery
{
    public int DeliveryId { get; set; }

    public int? OrderId { get; set; }

    public string? DeliveryStatus { get; set; }

    public DateOnly? DeliveryDate { get; set; }

    public virtual Order? Order { get; set; }
}
