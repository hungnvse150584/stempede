using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Cart
{
    public class CheckoutDto
    {
        [Required]
        public string PaymentMethodId { get; set; } // e.g., from Stripe

        [Required]
        public ShippingAddressDto ShippingAddress { get; set; }
    }
}
