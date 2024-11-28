using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Staff
{
    public int UserId { get; set; }

    public int? StaffPoint { get; set; }

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual ICollection<SupportRequest> SupportRequests { get; set; } = new List<SupportRequest>();

    public virtual User User { get; set; } = null!;
}
