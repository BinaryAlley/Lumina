#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.ValidatePath;

/// <summary>
/// API endpoint for the <c>/path/validate</c> route.
/// </summary>
public class ValidatePathEndpoint : BaseEndpoint<ValidatePathRequest, IResult>
{
    private readonly IQueryHandler<ValidatePathQuery, Result<PathValidResponse>> _validatePathQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpoint"/> class.
    /// </summary>
    /// <param name="validatePathQueryHandler">Injected service for handling validate path queries.</param>
    public ValidatePathEndpoint(IQueryHandler<ValidatePathQuery, Result<PathValidResponse>> validatePathQueryHandler)
    {
        _validatePathQueryHandler = validatePathQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Path.VALIDATE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Validates the validity of the file system path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path to validate.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ValidatePathRequest request, CancellationToken cancellationToken)
    {
        Result<PathValidResponse> result = await _validatePathQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
