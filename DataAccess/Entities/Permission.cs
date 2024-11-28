using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

public partial class Permission
{
    [Key]
    public int PermissionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PermissionName { get; set; } = null!;

    [Column(TypeName = "ntext")]
    public string? Description { get; set; }

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
