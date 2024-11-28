using BusinessLogic.DTOs.Product;
using BusinessLogic.DTOs;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Utils.Implementation;

namespace StempedeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a paginated list of products.
        /// </summary>
        /// <param name="queryParameters">Parameters for pagination, sorting, and filtering.</param>
        /// <returns>A paginated list of products.</returns>
        /// <response code="200">Returns the list of products.</response>
        /// <response code="400">If the query parameters are invalid.</response>
        [HttpGet("get-all-pagination")]
        [AllowAnonymous] 
        public async Task<ActionResult<ApiResponse<PaginatedList<ReadProductDto>>>> GetAllProducts([FromQuery] QueryParameters queryParameters)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid query parameters for GetAllProducts.");
                return BadRequest(new ApiResponse<IEnumerable<string>>
                {
                    Success = false,
                    Message = "Invalid query parameters.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var paginatedProducts = await _productService.GetAllProductsAsync(queryParameters);
            return Ok(new ApiResponse<PaginatedList<ReadProductDto>>
            {
                Success = true,
                Data = paginatedProducts,
                Message = "Products retrieved successfully."
            });
        }

        /// <summary>
        /// Retrieves a specific product by ID.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <returns>Product details.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous] 
        public async Task<ActionResult<ApiResponse<ReadProductDto>>> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", id);
                return NotFound(new ApiResponse<ReadProductDto>
                {
                    Success = false,
                    Message = $"Product with ID {id} not found.",
                    Data = null,
                    Errors = new List<string> { $"Product with ID {id} not found." }
                });
            }

            return Ok(new ApiResponse<ReadProductDto>
            {
                Success = true,
                Data = product,
                Message = "Product retrieved successfully.",
                Errors = null
            });
        }

        /// <summary>
        /// Create a new product.
        /// </summary>
        [HttpPost("create")]
        [Authorize(Roles = "Manager,Staff")]
        public async Task<ActionResult<ApiResponse<ReadProductDto>>> CreateProduct([FromBody] CreateProductDto createDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product creation attempt.");
                return BadRequest(new ApiResponse<ReadProductDto>
                {
                    Success = false,
                    Message = "Invalid product data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
                    Data = null
                });

            }

            try
            {
                var createdProduct = await _productService.CreateProductAsync(createDto);
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.ProductId }, new ApiResponse<ReadProductDto>
                {
                    Success = true,
                    Data = createdProduct,
                    Message = "Product created successfully.",
                    Errors = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Product creation failed due to invalid data.");
                return BadRequest(new ApiResponse<ReadProductDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message },
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a product.");
                return StatusCode(500, new ApiResponse<ReadProductDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the product.",
                    Errors = new List<string> { "Internal server error." },
                    Data = null
                });
            }
        }

        /// <summary>
        /// Update an existing product.
        /// </summary>
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Manager,Staff")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product update attempt for ID {ProductId}.", id);
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid update data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
                    Data = null
                });
            }

            try
            {
                var result = await _productService.UpdateProductAsync(id, updateDto);
                if (!result)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found.",
                        Data = null,
                        Errors = new List<string> { $"Product with ID {id} not found." }
                    });
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = "Product updated successfully.",
                    Message = "Product updated successfully.",
                    Errors = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Product update failed due to invalid data.");
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message },
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the product with ID {ProductId}.", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while updating the product.",
                    Errors = new List<string> { "Internal server error." },
                    Data = null
                });
            }
        }

        /// <summary>
        /// Delete a product by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found.",
                        Data = null,
                        Errors = new List<string> { $"Product with ID {id} not found." }
                    });
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = "Product deleted successfully.",
                    Message = "Product deleted successfully.",
                    Errors = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the product with ID {ProductId}.", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while deleting the product.",
                    Errors = new List<string> { "Internal server error." },
                    Data = null
                });
            }
        }
    }
}
