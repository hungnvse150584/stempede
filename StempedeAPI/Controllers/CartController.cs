using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Data;
using DataAccess.Entities;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.DTOs.Cart;
using BusinessLogic.DTOs;

namespace StempedeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CartController(ICartService cartService, IUnitOfWork unitOfWork, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the current user's active cart. If no active cart exists, returns an empty cart.
        /// </summary>
        /// <returns>An ApiResponse containing the cart details.</returns>
        /// <response code="200">Cart retrieved successfully.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("get-current-user")]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            var userName = User.Identity.Name;

            try
            {
                var cart = await _cartService.GetCartAsync(userName);
                if (cart == null)
                {
                    var user = await _unitOfWork.GetRepository<User>().FindAsync(u => u.Username == userName);
                    var userEntity = user.FirstOrDefault();

                    var emptyCart = new CartDto
                    {
                        CartId = 0,
                        UserId = userEntity?.UserId ?? 0,
                        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        Status = "Empty",
                        Items = new List<CartItemDto>(),
                        TotalAmount = 0m
                    };

                    return Ok(ApiResponse<CartDto>.SuccessResponse(emptyCart, "No active cart found. An empty cart has been created."));
                }

                return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "Cart retrieved successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument exception while retrieving cart for user: {UserName}", userName);
                return BadRequest(ApiResponse<string>.FailureResponse(ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving cart for user: {UserName}", userName);
                return StatusCode(500, ApiResponse<string>.FailureResponse("An error occurred while retrieving the cart.", new List<string> { "Internal server error." }));
            }
        }

        /// <summary>
        /// Adds a product to the user's cart.
        /// </summary>
        /// <param name="addItemDto">The product ID and quantity to add.</param>
        /// <returns>An ApiResponse indicating success or failure.</returns>
        /// <response code="201">Item added successfully.</response>
        /// <response code="400">Invalid input or insufficient stock.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("add-items")]
        public async Task<ActionResult<ApiResponse<string>>> AddItemToCart([FromBody] AddCartItemDto addItemDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid AddCartItemDto received.");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid input.", errors));
            }

            var userName = User.Identity.Name;

            try
            {

                var resultMessage = await _cartService.AddItemToCartAsync(userName, addItemDto.ProductId, addItemDto.Quantity);
                return CreatedAtAction(nameof(GetCart), new { }, ApiResponse<string>.SuccessResponse(resultMessage, "Item added to cart successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "AddItemToCartAsync failed for user: {UserName}. Reason: {Reason}", userName, ex.Message);
                return BadRequest(ApiResponse<string>.FailureResponse(ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user: {UserName}", userName);
                return StatusCode(500, ApiResponse<string>.FailureResponse("An error occurred while adding item to cart.", new List<string> { "Internal server error." }));
            }
        }

        /// <summary>
        /// Updates the quantity of a specific cart item.
        /// </summary>
        /// <param name="cartItemId">The ID of the cart item to update.</param>
        /// <param name="updateItemDto">The new quantity for the cart item.</param>
        /// <returns>An ApiResponse indicating success or failure.</returns>
        /// <response code="200">Cart item updated successfully.</response>
        /// <response code="400">Invalid input or insufficient stock.</response>
        /// <response code="404">Cart item not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("update-items-quantity/{cartItemId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto updateItemDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid UpdateCartItemDto received for CartItemID: {CartItemId}", cartItemId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid input.", errors));
            }

            var userName = User.Identity.Name;

            try
            {
                var resultMessage = await _cartService.UpdateCartItemAsync(userName, cartItemId, updateItemDto.Quantity);
                return Ok(ApiResponse<string>.SuccessResponse(resultMessage, "Cart item updated successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("UpdateCartItemAsync failed for CartItemID: {CartItemId}. Reason: {Reason}", cartItemId, ex.Message);
                return BadRequest(ApiResponse<string>.FailureResponse(ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for user: {UserName}, CartItemID: {CartItemId}", userName, cartItemId);
                return StatusCode(500, ApiResponse<string>.FailureResponse("An error occurred while updating the cart item.", new List<string> { "Internal server error." }));
            }
        }

        /// <summary>
        /// Removes a specific item from the user's cart.
        /// </summary>
        /// <param name="cartItemId">The ID of the cart item to remove.</param>
        /// <returns>An ApiResponse indicating success or failure.</returns>
        /// <response code="200">Item removed successfully.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="404">Cart item not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("remove-items/{cartItemId}")]
        public async Task<ActionResult<ApiResponse<string>>> RemoveItemFromCart(int cartItemId)
        {
            var userName = User.Identity.Name;

            try
            {
                var resultMessage = await _cartService.RemoveItemFromCartAsync(userName, cartItemId);
                return Ok(ApiResponse<string>.SuccessResponse(resultMessage, "Item removed from cart successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("RemoveItemFromCartAsync failed for CartItemID: {CartItemId}. Reason: {Reason}", cartItemId, ex.Message);
                return NotFound(ApiResponse<string>.FailureResponse(ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for user: {UserName}, CartItemID: {CartItemId}", userName, cartItemId);
                return StatusCode(500, ApiResponse<string>.FailureResponse("An error occurred while removing the cart item.", new List<string> { "Internal server error." }));
            }
        }

        /// <summary>
        /// Clears all items from the user's cart.
        /// </summary>
        /// <returns>An ApiResponse indicating success or failure.</returns>
        /// <response code="200">Cart cleared successfully.</response>
        /// <response code="404">Cart not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("clear-all-items")]
        public async Task<ActionResult<ApiResponse<string>>> ClearCart()
        {
            var userName = User.Identity.Name;

            try
            {
                var resultMessage = await _cartService.ClearCartAsync(userName);
                return Ok(ApiResponse<string>.SuccessResponse(resultMessage, "Cart cleared successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("ClearCartAsync failed for user: {UserName}. Reason: {Reason}", userName, ex.Message);
                return NotFound(ApiResponse<string>.FailureResponse(ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user: {UserName}", userName);
                return StatusCode(500, ApiResponse<string>.FailureResponse("An error occurred while clearing the cart.", new List<string> { "Internal server error." }));
            }
        }

        /// <summary>
        /// Converts the current cart into an order.
        /// </summary>
        /// <param name="checkoutDto">The checkout details including payment and shipping information.</param>
        /// <returns>An ApiResponse indicating success or failure.</returns>
        /// <response code="200">Checkout successful and order created.</response>
        /// <response code="400">Invalid input or insufficient stock.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("checkout")]
        public async Task<ActionResult<ApiResponse<string>>> Checkout([FromBody] CheckoutDto checkoutDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid CheckoutDto received.");
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid input.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(),
                    Data = null
                });
            }

            var userName = User.Identity.Name;

            try
            {
                var resultMessage = await _cartService.CheckoutAsync(userName, checkoutDto);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = resultMessage,
                    Message = "Checkout successful.",
                    Errors = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("CheckoutAsync failed for user: {UserName}. Reason: {Reason}", userName, ex.Message);
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for user: {UserName}", userName);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "An error occurred during checkout.",
                    Errors = new List<string> { "Internal server error." }
                });
            }
        }
    }
}
