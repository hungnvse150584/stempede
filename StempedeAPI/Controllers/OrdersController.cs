using BusinessLogic.DTOs.Order;
using BusinessLogic.DTOs;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Utils.Implementation;

namespace StempedeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all orders with pagination. Accessible by Managers only.
        /// </summary>
        /// <param name="queryParameters">Pagination parameters.</param>
        /// <returns>An ApiResponse containing a paginated list of orders.</returns>
        /// <response code="200">Orders retrieved successfully.</response>
        /// <response code="400">Bad request due to invalid parameters.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<ApiResponse<PaginatedList<OrderDto>>>> GetAllOrders([FromQuery] QueryParameters queryParameters)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid QueryParameters received.");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<PaginatedList<OrderDto>>.FailureResponse("Invalid pagination parameters.", errors));
            }

            try
            {
                var response = await _orderService.GetAllOrdersAsync(queryParameters);
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving all orders.");
                return StatusCode(500, ApiResponse<PaginatedList<OrderDto>>.FailureResponse("An error occurred while retrieving orders.", new List<string> { "Internal server error." }));
            }
        }

        /// <summary>
        /// Retrieves a specific order by ID. Accessible by Managers, Staff, and Customers.
        /// Customers can only access their own orders.
        /// </summary>
        /// <param name="id">The ID of the order to retrieve.</param>
        /// <returns>An ApiResponse containing the order details.</returns>
        /// <response code="200">Order retrieved successfully.</response>
        /// <response code="403">Forbidden - Access denied.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Manager,Staff,Customer")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(int id)
        {
            var userName = User.Identity.Name;
            var userRole = User.IsInRole("Manager") ? "Manager" :
                           User.IsInRole("Staff") ? "Staff" :
                           "Customer";

            try
            {
                var response = await _orderService.GetOrderByIdAsync(id, userName, userRole);
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    if (response.Message == "Order not found.")
                        return NotFound(response);
                    else if (response.Message == "Access denied.")
                        return Forbid();
                    else
                        return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving order with ID: {OrderId}", id);
                return StatusCode(500, ApiResponse<OrderDto>.FailureResponse("An error occurred while retrieving the order.", new List<string> { "Internal server error." }));
            }
        }

        /// <summary>
        /// Updates the delivery status of a specific delivery. Accessible by Staff only.
        /// </summary>
        /// <param name="orderId">The ID of the order associated with the delivery.</param>
        /// <param name="deliveryId">The ID of the delivery to update.</param>
        /// <param name="updateDto">The new delivery status and date.</param>
        /// <returns>An ApiResponse indicating the result of the operation.</returns>
        /// <response code="204">Delivery status updated successfully.</response>
        /// <response code="400">Invalid input or unauthorized access.</response>
        /// <response code="404">Delivery not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{orderId}/deliveries/{deliveryId}")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateDeliveryStatus(int orderId, int deliveryId, [FromBody] UpdateDeliveryStatusDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid UpdateDeliveryStatusDto received for OrderId: {OrderId}, DeliveryId: {DeliveryId}", orderId, deliveryId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid input.", errors));
            }

            var userRole = "Staff"; // Since only Staff can access this endpoint

            try
            {
                var response = await _orderService.UpdateDeliveryStatusAsync(orderId, deliveryId, updateDto, userRole);
                if (response.Success)
                {
                    return NoContent(); // 204 No Content for successful PUT without response body
                }
                else
                {
                    if (response.Message == "Delivery not found.")
                        return NotFound(response);
                    else if (response.Message == "Unauthorized access.")
                        return Forbid();
                    else
                        return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating delivery status for OrderId: {OrderId}, DeliveryId: {DeliveryId}", orderId, deliveryId);
                return StatusCode(500, ApiResponse<string>.FailureResponse("An error occurred while updating delivery status.", new List<string> { "Internal server error." }));
            }
        }

        ///// <summary>
        ///// Generates a sales report for a specified date range. Accessible by Managers only.
        ///// </summary>
        ///// <param name="fromDate">The start date for the report.</param>
        ///// <param name="toDate">The end date for the report.</param>
        ///// <returns>An ApiResponse containing the sales report.</returns>
        ///// <response code="200">Sales report generated successfully.</response>
        ///// <response code="500">Internal server error.</response>
        //[HttpGet("Report/Sales")]
        //[Authorize(Roles = "Manager")]
        //public async Task<ActionResult<ApiResponse<SalesReportDto>>> GetSalesReport([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        //{
        //    try
        //    {
        //        var response = await _orderService.GetSalesReportAsync(fromDate, toDate);
        //        if (response.Success)
        //        {
        //            return Ok(response);
        //        }
        //        else
        //        {
        //            return StatusCode(500, response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error while generating sales report.");
        //        return StatusCode(500, ApiResponse<SalesReportDto>.FailureResponse("An error occurred while generating the sales report.", new List<string> { "Internal server error." }));
        //    }
        //}
    }
}

