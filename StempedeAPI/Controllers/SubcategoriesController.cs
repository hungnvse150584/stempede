using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.DTOs.Subcategory;
using BusinessLogic.DTOs;

namespace StempedeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubcategoriesController : ControllerBase
    {
        private readonly ISubcategoryService _subcategoryService;
        private readonly ILogger<SubcategoriesController> _logger;

        public SubcategoriesController(ISubcategoryService subcategoryService, ILogger<SubcategoriesController> logger)
        {
            _subcategoryService = subcategoryService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all subcategories.
        /// </summary>
        /// <returns>List of subcategories.</returns>
        /// <response code="200">Returns the list of subcategories.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet("get-all")]
        [AllowAnonymous] // Allows unauthenticated access if desired; remove if not
        public async Task<ActionResult<ApiResponse<IEnumerable<ReadSubcategoryDto>>>> GetAllSubcategories()
        {
            try
            {
                var subcategories = await _subcategoryService.GetAllSubcategoriesAsync();
                return Ok(new ApiResponse<IEnumerable<ReadSubcategoryDto>>
                {
                    Success = true,
                    Data = subcategories,
                    Message = "Subcategories retrieved successfully.",
                    Errors = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving subcategories.");
                return StatusCode(500, new ApiResponse<IEnumerable<ReadSubcategoryDto>>
                {
                    Success = false,
                    Data = null,
                    Message = "An error occurred while retrieving subcategories.",
                    Errors = new List<string> { "Internal server error." }
                });
            }
        }
    }
}
