using Microsoft.AspNetCore.Mvc;
using QIM.Shared.Models;

namespace QIM.Presentation.Endpoints;

/// <summary>
/// Base controller that converts Result{T} to proper HTTP status codes.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Converts a Result to the appropriate IActionResult (200/400).
    /// </summary>
    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess) return Ok(result);
        if (result.IsForbidden) return StatusCode(403, result);
        if (result.IsNotFound || HasNotFoundError(result)) return NotFound(result);
        return BadRequest(result);
    }

    /// <summary>
    /// Converts a Result{T} to the appropriate IActionResult (200/400/403/404).
    /// </summary>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess) return Ok(result);
        if (result.IsForbidden) return StatusCode(403, result);
        if (result.IsNotFound || HasNotFoundError(result)) return NotFound(result);
        return BadRequest(result);
    }

    // DEF-NEW-009: handlers historically use Result.Failure("... was not found.") for missing entities.
    // Map those to HTTP 404 so callers can distinguish missing resources from validation errors.
    private static bool HasNotFoundError(Result result)
        => result.Errors.Count > 0 &&
           result.Errors.Any(e => e is not null && e.Contains("not found", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns 404 with a standard message.
    /// </summary>
    protected IActionResult NotFoundResult(string entity, int id)
    {
        return NotFound(Result.Failure($"{entity} with Id {id} was not found."));
    }
}
