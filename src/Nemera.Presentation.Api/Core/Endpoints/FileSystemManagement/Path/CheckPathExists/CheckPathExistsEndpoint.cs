#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;

/// <summary>
/// API endpoint for the <c>/path/check-path-exists</c> route.
/// </summary>
public class CheckPathExistsEndpoint : BaseEndpoint<CheckPathExistsRequest, IResult>
{
    private readonly IQueryHandler<CheckPathExistsQuery, Result<PathExistsResponse>> _checkPathExistsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsEndpoint"/> class.
    /// </summary>
    /// <param name="checkPathExistsQueryHandler">Injected service for handling check path exists queries.</param>
    public CheckPathExistsEndpoint(IQueryHandler<CheckPathExistsQuery, Result<PathExistsResponse>> checkPathExistsQueryHandler)
    {
        _checkPathExistsQueryHandler = checkPathExistsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Path.CHECK_PATH_EXISTS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Checks the existence of the file system path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path whose existence is checked.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CheckPathExistsRequest request, CancellationToken cancellationToken)
    {
        Result<PathExistsResponse> result = await _checkPathExistsQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
