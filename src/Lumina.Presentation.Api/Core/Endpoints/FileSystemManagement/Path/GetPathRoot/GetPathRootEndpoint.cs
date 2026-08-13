#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathRoot;

/// <summary>
/// API endpoint for the <c>/path/get-path-root</c> route.
/// </summary>
public class GetPathRootEndpoint : BaseEndpoint<GetPathRootRequest, IResult>
{
    private readonly IQueryHandler<GetPathRootQuery, ErrorOr<PathSegmentResponse>> _getPathRootQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootEndpoint"/> class.
    /// </summary>
    /// <param name="getPathRootQueryHandler">Injected service for handling get path root queries.</param>
    public GetPathRootEndpoint(IQueryHandler<GetPathRootQuery, ErrorOr<PathSegmentResponse>> getPathRootQueryHandler)
    {
        _getPathRootQueryHandler = getPathRootQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Path.GET_PATH_ROOT);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the root of the file system path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path for which to get the root.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetPathRootRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<PathSegmentResponse> result = await _getPathRootQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
