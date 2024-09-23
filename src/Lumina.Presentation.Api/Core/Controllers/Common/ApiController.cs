#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Presentation.Api.Common.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.Common;

/// <summary>
/// Base class for API controllers.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
public abstract class ApiController : ControllerBase
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Handles controllers errors.
    /// </summary>
    /// <param name="errors">The error to handle.</param>
    protected internal IActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0) // if there are no custom errors given, return a generic problem
            return Problem();
        // try to make the problem type more specific
        if (errors.All(error => error.Type == ErrorType.Validation))
            return ValidationProblem(errors);
        // here we could handle custom error types, for instance: if (errors.All(error => error.NumericType == 23)), where 23 could be "domain errors"

        // share the list of errors within the scope of this HTTP request
        HttpContext.Items[HttpContextItemKeys.ERRORS] = errors;
        // get a HTTP error status code for the selected domain error type
        return Problem(errors.First());
    }

    /// <summary>
    /// Converts an <see cref="Error"/> to a standard problem response.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    protected internal IActionResult Problem(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Failure => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,

        };
        // return the HTTP error status and the domain error message
        return Problem(statusCode: statusCode, title: error.Type.ToString(), detail: error.Description);
    }

    /// <summary>
    /// Converts a list of <see cref="Error"/> into a standard validation problem response.
    /// </summary>
    /// <param name="errors">The list of errors to convert.</param>
    protected internal IActionResult ValidationProblem(List<Error> errors)
    {
        ModelStateDictionary modelStateDictionary = new();
        foreach (Error error in errors)
            modelStateDictionary.AddModelError(error.Code, error.Description);
        return ValidationProblem(modelStateDictionary: modelStateDictionary, title: "One or more validation errors occurred.");
    }
    #endregion
}