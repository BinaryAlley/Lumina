#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Nemera.Contracts.Requests.MediaLibrary.Management;
using Nemera.Contracts.Responses.MediaLibrary.Management;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.Management;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.Management.GetLibraryScanProgress;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/scans/{scanId}/progress</c> route.
/// </summary>
public class GetLibraryScanProgressEndpoint : BaseEndpoint<GetLibraryScanProgressRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryScanProgressQuery, Result<MediaLibraryScanProgressResponse>> _getLibraryScanProgressQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryScanProgressEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryScanProgressQueryHandler">Injected service for handling get library scan progress queries.</param>
    public GetLibraryScanProgressEndpoint(IQueryHandler<GetLibraryScanProgressQuery, Result<MediaLibraryScanProgressResponse>> getLibraryScanProgressQueryHandler)
    {
        _getLibraryScanProgressQueryHandler = getLibraryScanProgressQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Libraries.LIBRARY_SCAN_PROGRESS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the progress of a media library scan.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library and of the scan whose progress is requested.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetLibraryScanProgressRequest request, CancellationToken cancellationToken)
    {
        Result<MediaLibraryScanProgressResponse> result = await _getLibraryScanProgressQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
