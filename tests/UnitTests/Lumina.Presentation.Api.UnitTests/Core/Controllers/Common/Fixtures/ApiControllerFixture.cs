#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Presentation.Api.Core.Controllers.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Controllers.Common.Fixtures;

/// <summary>
/// Fixture class for the <see cref="ApiController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApiControllerFixture : ApiController
{
    /// <summary>
    /// Returns a standardized error response using the <see cref="ProblemDetails"/> format.
    /// </summary>
    /// <param name="errors">A list of errors to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> with the problem details containing the specified errors.</returns>
    public IActionResult TestProblem(List<Error> errors)
    {
        return Problem(errors);
    }

    /// <summary>
    /// Returns a standardized validation error response using the <see cref="ValidationProblemDetails"/> format.
    /// </summary>
    /// <param name="errors">A list of validation errors to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> with the validation problem details containing the specified errors.</returns>
    public IActionResult TestValidationProblem(List<Error> errors)
    {
        return ValidationProblem(errors);
    }
}
