using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.DTOs.Order;
using BusinessLogic.DTOs.Reporting;
using BusinessLogic.DTOs;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.Utils.Implementation;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedList<OrderDto>>> GetAllOrdersAsync(QueryParameters queryParameters)
        {
            try
            {
                _logger.LogInformation("Fetching all orders for reporting with pagination. PageNumber: {PageNumber}, PageSize: {PageSize}",
                    queryParameters.PageNumber, queryParameters.PageSize);

                var orderQuery = _unitOfWork.GetRepository<Order>()
                                            .GetAllQueryable(includeProperties: "User,Deliveries,OrderDetails.Product")
                                            .OrderBy(o => o.OrderId); // Ensure consistent ordering

                var paginatedOrders = await PaginatedList<Order>.CreateAsync(orderQuery, queryParameters.PageNumber, queryParameters.PageSize);

                var orderDtos = _mapper.Map<List<OrderDto>>(paginatedOrders.Items);

                var paginatedOrderDtos = new PaginatedList<OrderDto>(orderDtos, paginatedOrders.TotalCount, paginatedOrders.PageIndex, queryParameters.PageSize);

                return ApiResponse<PaginatedList<OrderDto>>.SuccessResponse(paginatedOrderDtos, "Orders retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all orders.");
                return ApiResponse<PaginatedList<OrderDto>>.FailureResponse("Failed to retrieve orders.", new List<string> { ex.Message });
            }
        }


        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int orderId, string currentUsername, string userRole)
        {
            try
            {
                _logger.LogInformation("Fetching order with ID: {OrderId}", orderId);

                var order = await _unitOfWork.GetRepository<Order>()
                                             .GetAllQueryable(includeProperties: "User,Deliveries,OrderDetails.Product")
                                             .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID: {OrderId} not found.", orderId);
                    return ApiResponse<OrderDto>.FailureResponse("Order not found.", new List<string> { "The specified order does not exist." });
                }

                // If the user is a Customer, ensure they own the order
                if (userRole.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    if (!order.User.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("User {Username} attempted to access Order ID: {OrderId} which does not belong to them.", currentUsername, orderId);
                        return ApiResponse<OrderDto>.FailureResponse("Access denied.", new List<string> { "You do not have permission to access this order." });
                    }
                }

                var orderDto = _mapper.Map<OrderDto>(order);
                return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order with ID: {OrderId}", orderId);
                return ApiResponse<OrderDto>.FailureResponse("Failed to retrieve the order.", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<string>> UpdateDeliveryStatusAsync(int orderId, int deliveryId, UpdateDeliveryStatusDto updateDto, string userRole)
        {
            if (!userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Unauthorized attempt to update delivery status by user role: {UserRole}", userRole);
                return ApiResponse<string>.FailureResponse("Unauthorized access.", new List<string> { "You do not have permission to perform this action." });
            }

            try
            {
                _logger.LogInformation("Updating delivery status for Order ID: {OrderId}, Delivery ID: {DeliveryId}", orderId, deliveryId);

                var deliveryRepository = _unitOfWork.GetRepository<Delivery>();
                var delivery = await deliveryRepository.GetAsync(d => d.DeliveryId == deliveryId && d.OrderId == orderId);

                if (delivery == null)
                {
                    _logger.LogWarning("Delivery with ID: {DeliveryId} for Order ID: {OrderId} not found.", deliveryId, orderId);
                    return ApiResponse<string>.FailureResponse("Delivery not found.", new List<string> { "The specified delivery does not exist." });
                }

                delivery.DeliveryStatus = updateDto.DeliveryStatus;
                delivery.DeliveryDate = updateDto.DeliveryDate;

                deliveryRepository.Update(delivery);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Delivery status updated successfully for Delivery ID: {DeliveryId}", deliveryId);
                return ApiResponse<string>.SuccessResponse("Delivery status updated successfully.", "Delivery status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status for Delivery ID: {DeliveryId}", deliveryId);
                return ApiResponse<string>.FailureResponse("Failed to update delivery status.", new List<string> { ex.Message });
            }
        }

        //public async Task<ApiResponse<SalesReportDto>> GetSalesReportAsync(DateOnly fromDate, DateOnly toDate)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Generating sales report from {FromDate} to {ToDate}", fromDate, toDate);

        //        var orders = await _unitOfWork.GetRepository<Order>()
        //                                       .GetAllQueryable(includeProperties: "User")
        //                                       .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
        //                                       .ToListAsync();

        //        var reportDto = new SalesReportDto
        //        {
        //            FromDate = fromDate,
        //            ToDate = toDate,
        //            TotalSales = orders.Sum(o => o.TotalAmount ?? 0),
        //            TotalOrders = orders.Count,
        //            Details = orders.Select(o => new SalesReportDetailDto
        //            {
        //                OrderDate = o.OrderDate ?? DateOnly.MinValue,
        //                OrderId = o.OrderId,
        //                CustomerUsername = o.User.Username,
        //                OrderTotal = o.TotalAmount ?? 0
        //            }).ToList()
        //        };

        //        return ApiResponse<SalesReportDto>.SuccessResponse(reportDto, "Sales report generated successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error generating sales report.");
        //        return ApiResponse<SalesReportDto>.FailureResponse("Failed to generate sales report.", new List<string> { ex.Message });
        //    }
        //}
    }
}