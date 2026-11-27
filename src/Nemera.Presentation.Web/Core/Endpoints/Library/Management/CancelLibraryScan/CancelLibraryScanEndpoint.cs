#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Requests.Library.Management;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.Management.CancelLibraryScan;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/{libraryId}/api-cancel-library-scan/{scanId}</c> route.
/// </summary>
public class CancelLibraryScanEndpoint : BaseEndpoint<CancelLibraryScanRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public CancelLibraryScanEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.LibraryManagement.CANCEL_LIBRARY_SCAN);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Cancels a running or queued scan of a media library.
    /// </summary>
    /// <param name="request">The request containing the Ids of the media library and of the scan to cancel.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CancelLibraryScanRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PostIgnoringResponseAsync<CancelLibraryScanRequest>(ApiRoutes.Libraries.CANCEL_LIBRARY_SCAN.Replace("{libraryId}", request.LibraryId.ToString()).Replace("{scanId}", request.ScanId.ToString()), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
