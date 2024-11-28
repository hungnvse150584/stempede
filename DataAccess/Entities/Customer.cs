using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Customer
{
    public int UserId { get; set; }

    public DateTime RegistrationDate { get; set; }

    public int? CustomerPoint { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<SupportRequest> SupportRequests { get; set; } = new List<SupportRequest>();

    public virtual User User { get; set; } = null!;
}
