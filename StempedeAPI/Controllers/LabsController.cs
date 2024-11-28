using BusinessLogic.DTOs.Lab;
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
    public class LabsController : ControllerBase
    {
        private readonly ILabService _labService;
        private readonly ILogger<LabsController> _logger;

        public LabsController(ILabService labService, ILogger<LabsController> logger)
        {
            _labService = labService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all labs.
        /// </summary>
        /// <returns>List of labs.</returns>
        /// <response code="200">Returns the list of labs.</response>
        [HttpGet("get-all")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReadLabSimpleDto>>>> GetAllLabs()
        {
            var labs = await _labService.GetAllLabsAsync();

            return Ok(new ApiResponse<IEnumerable<ReadLabSimpleDto>>
            {
                Success = true,
                Data = labs,
                Message = "Labs retrieved successfully."
            });
        }

        /// <summary>
        /// Retrieves paginated list of labs.
        /// </summary>
        /// <param name="queryParameters">Parameters for pagination.</param>
        /// <returns>Paginated list of labs.</returns>
        /// <response code="200">Returns the paginated list of labs.</response>
        /// <response code="400">If the query parameters are invalid.</response>
        [HttpGet("get-all-pagination")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PaginatedList<ReadLabSimpleDto>>>> GetAllLabsPaginated([FromQuery] QueryParameters queryParameters)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid query parameters for GetAllLabsPaginated.");
                return BadRequest(new ApiResponse<IEnumerable<string>>
                {
                    Success = false,
                    Message = "Invalid query parameters.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var paginatedLabs = await _labService.GetLabsAsync(queryParameters);

            return Ok(new ApiResponse<PaginatedList<ReadLabSimpleDto>>
            {
                Success = true,
                Data = paginatedLabs,
                Message = "Labs retrieved successfully."
            });
        }

        /// <summary>
        /// Retrieves a specific lab by ID.
        /// </summary>
        /// <param name="id">Lab ID.</param>
        /// <returns>Lab details.</returns>
        [HttpGet("{id}", Name = "GetLabById")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ReadLabSimpleDto>>> GetLabById(int id)
        {
            var lab = await _labService.GetLabByIdAsync(id);
            if (lab == null)
            {
                _logger.LogWarning("Lab with ID {LabId} not found.", id);
                return NotFound(new ApiResponse<ReadLabSimpleDto>
                {
                    Success = false,
                    Message = $"Lab with ID {id} not found.",
                    Data = null,
                    Errors = new List<string> { $"Lab with ID {id} not found." }
                });
            }
            return Ok(new ApiResponse<ReadLabSimpleDto>
            {
                Success = true,
                Data = lab,
                Message = "Lab retrieved successfully."
            });
        }

        /// <summary>
        /// Creates a new lab.
        /// </summary>
        /// <param name="createLabDto">Lab details.</param>
        /// <returns>Created lab.</returns>
        [HttpPost("create")]
        
        public async Task<ActionResult<ApiResponse<ReadLabSimpleDto>>> CreateLab([FromBody] CreateLabDto createLabDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreateLab.");
                return BadRequest(new ApiResponse<ReadLabSimpleDto>
                {
                    Success = false,
                    Message = "Invalid lab data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
                    Data = null
                });
            }
            try
            {
                var createdLab = await _labService.CreateLabAsync(createLabDto);
                return CreatedAtRoute(nameof(GetLabById), new { id = createdLab.LabId }, new ApiResponse<ReadLabSimpleDto>
                {
                    Success = true,
                    Data = createdLab,
                    Message = "Lab created successfully."
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Lab creation failed due to invalid data.");
                return BadRequest(new ApiResponse<ReadLabSimpleDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message },
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a lab.");
                return StatusCode(500, new ApiResponse<ReadLabSimpleDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the lab.",
                    Errors = new List<string> { "Internal server error." },
                    Data = null
                });
            }
        }

        /// <summary>
        /// Updates an existing lab.
        /// </summary>
        /// <param name="id">Lab ID.</param>
        /// <param name="updateLabDto">Updated lab details.</param>
        /// <returns>No content.</returns>
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Manager,Staff")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateLab(int id, [FromBody] UpdateLabDto updateLabDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateLab.");
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid update data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
                    Data = null
                });
            }

            var result = await _labService.UpdateLabAsync(id, updateLabDto);
            if (!result)
            {
                _logger.LogWarning("Lab with ID {LabId} not found for update.", id);
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Lab with ID {id} not found.",
                    Data = null,
                    Errors = new List<string> { $"Lab with ID {id} not found." }
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = "Lab updated successfully.",
                Message = "Lab updated successfully."
            });
        }


        /// <summary>
        /// Deletes a lab by ID.
        /// </summary>
        /// <param name="id">Lab ID.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteLab(int id)
        {
            try
            {
                var result = await _labService.DeleteLabAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Lab with ID {id} not found.",
                        Data = null,
                        Errors = new List<string> { $"Lab with ID {id} not found." }
                    });
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = "Lab deleted successfully.",
                    Message = "Lab deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the lab with ID {LabId}.", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while deleting the lab.",
                    Errors = new List<string> { "Internal server error." },
                    Data = null
                });
            }
        }
    }
}
