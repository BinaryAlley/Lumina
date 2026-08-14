#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathParent;

/// <summary>
/// API endpoint for the <c>/path/get-path-parent</c> route.
/// </summary>
public class GetPathParentEndpoint : BaseEndpoint<GetPathParentRequest, IResult>
{
    private readonly IQueryHandler<GetPathParentQuery, Result<IEnumerable<PathSegmentResponse>>> _getPathParentQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentEndpoint"/> class.
    /// </summary>
    /// <param name="getPathParentQueryHandler">Injected service for handling get path parent queries.</param>
    public GetPathParentEndpoint(IQueryHandler<GetPathParentQuery, Result<IEnumerable<PathSegmentResponse>>> getPathParentQueryHandler)
    {
        _getPathParentQueryHandler = getPathParentQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Path.GET_PATH_PARENT);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the parent directory of the file system path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path for which to get the parent directory.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetPathParentRequest request, CancellationToken cancellationToken)
    {
        Result<IEnumerable<PathSegmentResponse>> result = await _getPathParentQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
