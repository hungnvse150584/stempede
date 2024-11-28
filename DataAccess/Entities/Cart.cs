using DataAccess.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

public partial class Cart
{
    [Key]
    public int CartId { get; set; }

    public int UserId { get; set; }

    public DateOnly CreatedDate { get; set; }

    public string Status { get; set; } = CartStatusConstants.Active;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
