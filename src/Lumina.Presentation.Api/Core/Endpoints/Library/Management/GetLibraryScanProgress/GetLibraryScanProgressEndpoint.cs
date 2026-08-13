#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibraryScanProgress;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/scans/{scanId}/progress</c> route.
/// </summary>
public class GetLibraryScanProgressEndpoint : BaseEndpoint<GetLibraryScanProgressRequest, IResult>
{
    private readonly IQueryHandler<GetLibraryScanProgressQuery, ErrorOr<MediaLibraryScanProgressResponse>> _getLibraryScanProgressQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryScanProgressEndpoint"/> class.
    /// </summary>
    /// <param name="getLibraryScanProgressQueryHandler">Injected service for handling get library scan progress queries.</param>
    public GetLibraryScanProgressEndpoint(IQueryHandler<GetLibraryScanProgressQuery, ErrorOr<MediaLibraryScanProgressResponse>> getLibraryScanProgressQueryHandler)
    {
        _getLibraryScanProgressQueryHandler = getLibraryScanProgressQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
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
        ErrorOr<MediaLibraryScanProgressResponse> result = await _getLibraryScanProgressQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
