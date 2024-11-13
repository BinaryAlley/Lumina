#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Common.Fixtures;

/// <summary>
/// Fixture class for the <see cref="BaseEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BaseEndpointFixture : BaseEndpoint<object, object>
{
    /// <summary>
    /// Returns a standardized error response using the <see cref="ProblemDetails"/> format.
    /// </summary>
    /// <param name="errors">A list of errors to include in the response.</param>
    /// <returns>An <see cref="IResult"/> with the problem details containing the specified errors.</returns>
    public IResult TestProblem(List<Error> errors)
    {
        return Problem(errors);
    }

    /// <summary>
    /// Returns a standardized validation error response using the <see cref="ValidationProblemDetails"/> format.
    /// </summary>
    /// <param name="errors">A list of validation errors to include in the response.</param>
    /// <returns>An <see cref="IResult"/> with the validation problem details containing the specified errors.</returns>
    public IResult TestValidationProblem(List<Error> errors)
    {
        return ValidationProblem(errors);
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
    
    }

    /// <summary>
    /// Handles the API request.
    /// </summary>
    /// <param name="request">The request containing the file system path for which to get the root.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<object> ExecuteAsync(object request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new object());
    }
}
