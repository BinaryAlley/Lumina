#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Common.Routes.Library.Management;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
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
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryScanProgressEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetLibraryScanProgressEndpoint(ISender sender)
    {
        _sender = sender;
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
        ErrorOr<MediaLibraryScanProgressResponse> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
