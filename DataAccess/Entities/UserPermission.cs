using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

public partial class UserPermission
{
    [Key]
    public int UserPermissionId { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }

    [ForeignKey("Permission")]
    public int PermissionId { get; set; }

    public int AssignedBy { get; set; }

    public virtual User AssignedByNavigation { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
