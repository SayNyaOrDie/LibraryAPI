using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces;
using WebApplication3.Exceptions;
using WebApplication3.DTOs;
using Microsoft.Extensions.Logging;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorsController : ControllerBase
    {
        private readonly IVisitorService _visitorService;
        private readonly ILogger<VisitorsController> _logger;

        public VisitorsController(IVisitorService visitorService, ILogger<VisitorsController> logger)
        {
            _visitorService = visitorService;
            _logger = logger;
        }
        private bool IsInternetExplorer()
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            return userAgent.Contains("MSIE") || userAgent.Contains("Trident/");
        }

        [HttpPost("addNewVisitor")]
        public IActionResult AddVisitor([FromBody] VisitorDTO visitorDTO)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });

                if (visitorDTO == null)
                {
                    _logger.LogWarning("Invalid data received for AddVisitor.");
                    return BadRequest(new { message = "Invalid data" });
                }

                var visitor = new Visitor(
                    visitorDTO.Name,
                    visitorDTO.Surname,
                    visitorDTO.Email
                );

                _visitorService.AddVisitor(visitor);
                _logger.LogInformation($"Visitor with ID {visitor.Id} has been successfully added.");

                return CreatedAtAction(nameof(GetVisitorById), new { id = visitor.Id }, visitor);
            }
            catch (LibraryException ex)
            {
                _logger.LogError($"Error in AddVisitor: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in AddVisitor: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetVisitorById(int id)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });

                var visitor = _visitorService.GetVisitorById(id);
                if (visitor == null)
                {
                    _logger.LogWarning($"Visitor with ID {id} not found.");
                    return NotFound(new { message = $"Visitor with ID {id} not found" });
                }
                _logger.LogInformation($"Visitor with ID {id} retrieved successfully.");
                return Ok(visitor);
            }
            catch (LibraryException ex)
            {
                _logger.LogError($"Error retrieving visitor with ID {id}: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in GetVisitorById for ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpGet("getAllVisitors")]
        public IActionResult GetAllVisitors()
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });

                var visitors = _visitorService.GetAllVisitors();
                _logger.LogInformation("Retrieved all visitors.");
                return Ok(visitors);
            }
            catch (LibraryException ex)
            {
                _logger.LogError($"Error retrieving all visitors: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in GetAllVisitors: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpPut("{id}/update")]
        public IActionResult UpdateVisitorDetails(int id, [FromBody] VisitorDTO visitorDTO)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });

                if (visitorDTO == null)
                {
                    _logger.LogWarning("Visitor data is missing or invalid for update.");
                    return BadRequest(new { message = "Visitor data is missing or invalid" });
                }

                var existingVisitor = _visitorService.GetVisitorById(id);
                if (existingVisitor == null)
                {
                    _logger.LogWarning($"Visitor with ID {id} not found for update.");
                    return NotFound(new { message = $"Visitor with ID {id} not found" });
                }

                existingVisitor.FirstName = visitorDTO.Name;
                existingVisitor.LastName = visitorDTO.Surname;
                existingVisitor.Email = visitorDTO.Email;
                _visitorService.UpdateVisitorDetails(id, existingVisitor);

                _logger.LogInformation($"Visitor with ID {id} has been successfully updated.");
                return Ok(new { message = $"Visitor with ID {id} has been successfully updated" });
            }
            catch (LibraryException ex)
            {
                _logger.LogError($"Error updating visitor with ID {id}: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in UpdateVisitorDetails for ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpPost("{id}/payOffDebt")]
        public IActionResult PayOffDebt(int id)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });

                var visitor = _visitorService.GetVisitorById(id);
                if (visitor == null)
                {
                    _logger.LogWarning($"Visitor with ID {id} not found for debt payment.");
                    return NotFound(new { message = $"Visitor with ID {id} not found" });
                }

                _visitorService.PayOffDebt(id);
                _logger.LogInformation($"Debt for visitor with ID {id} has been successfully paid off.");

                return Ok(new { message = $"Debt for visitor with ID {id} has been successfully paid off" });
            }
            catch (LibraryException ex)
            {
                _logger.LogError($"Error paying off debt for visitor with ID {id}: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in PayOffDebt for ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteVisitor(int id)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });

                var visitor = _visitorService.GetVisitorById(id);
                if (visitor == null)
                {
                    _logger.LogWarning($"Visitor with ID {id} not found for deletion.");
                    return NotFound(new { message = $"Visitor with ID {id} not found" });
                }

                _visitorService.DeleteVisitor(visitor);
                _logger.LogInformation($"Visitor with ID {id} has been successfully deleted.");

                return Ok(new { message = $"Visitor with ID {id} has been successfully deleted" });
            }
            catch (LibraryException ex)
            {
                _logger.LogError($"Error deleting visitor with ID {id}: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in DeleteVisitor for ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }
    }
}
