using BusinessLogic.DTOs.Cart;

namespace BusinessLogic.Services.Interfaces
{
    public interface ICartService
    {
        /// <summary>
        /// Retrieves the current user's active cart.
        /// </summary>
        /// <param name="userName">The username of the authenticated user.</param>
        /// <returns>A CartDto representing the user's cart.</returns>
        Task<CartDto> GetCartAsync(string userName);

        /// <summary>
        /// Adds a product to the user's cart.
        /// </summary>
        /// <param name="userName">The username of the authenticated user.</param>
        /// <param name="productId">The ID of the product to add.</param>
        /// <param name="quantity">The quantity of the product to add.</param>
        /// <returns>A success message indicating the item was added.</returns>
        Task<string> AddItemToCartAsync(string userName, int productId, int quantity);

        /// <summary>
        /// Updates the quantity of a specific cart item.
        /// </summary>
        /// <param name="userName">The username of the authenticated user.</param>
        /// <param name="cartItemId">The ID of the cart item to update.</param>
        /// <param name="quantity">The new quantity for the cart item.</param>
        /// <returns>A success message indicating the item was updated.</returns>
        Task<string> UpdateCartItemAsync(string userName, int cartItemId, int quantity);

        /// <summary>
        /// Removes a specific item from the user's cart.
        /// </summary>
        /// <param name="userName">The username of the authenticated user.</param>
        /// <param name="cartItemId">The ID of the cart item to remove.</param>
        /// <returns>A success message indicating the item was removed.</returns>
        Task<string> RemoveItemFromCartAsync(string userName, int cartItemId);

        /// <summary>
        /// Clears all items from the user's cart.
        /// </summary>
        /// <param name="userName">The username of the authenticated user.</param>
        /// <returns>A success message indicating the cart was cleared.</returns>
        Task<string> ClearCartAsync(string userName);

        /// <summary>
        /// Converts the current cart into an order.
        /// </summary>
        /// <param name="userName">The username of the authenticated user.</param>
        /// <param name="checkoutDto">The checkout details including payment and shipping information.</param>
        /// <returns>A success message indicating the order was created.</returns>
        Task<string> CheckoutAsync(string userName, CheckoutDto checkoutDto);
    }
}
