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
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Converts a Result{T} to the appropriate IActionResult (200/400).
    /// </summary>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Returns 404 with a standard message.
    /// </summary>
    protected IActionResult NotFoundResult(string entity, int id)
    {
        return NotFound(Result.Failure($"{entity} with Id {id} was not found."));
    }
}
