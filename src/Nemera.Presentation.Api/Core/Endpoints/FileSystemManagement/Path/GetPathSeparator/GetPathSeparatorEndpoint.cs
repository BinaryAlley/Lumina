#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathSeparator;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;

/// <summary>
/// API endpoint for the <c>/path/get-path-separator</c> route.
/// </summary>
public class GetPathSeparatorEndpoint : EndpointWithoutRequest<IResult>
{
    private readonly IQueryHandler<GetPathSeparatorQuery, PathSeparatorResponse> _getPathSeparatorQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpoint"/> class.
    /// </summary>
    /// <param name="getPathSeparatorQueryHandler">Injected service for handling get path separator queries.</param>
    public GetPathSeparatorEndpoint(IQueryHandler<GetPathSeparatorQuery, PathSeparatorResponse> getPathSeparatorQueryHandler)
    {
        _getPathSeparatorQueryHandler = getPathSeparatorQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Path.GET_PATH_SEPARATOR);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the file system path separator character of the file system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        PathSeparatorResponse result = await _getPathSeparatorQueryHandler.HandleAsync(new GetPathSeparatorQuery(), cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(result);
    }
}
