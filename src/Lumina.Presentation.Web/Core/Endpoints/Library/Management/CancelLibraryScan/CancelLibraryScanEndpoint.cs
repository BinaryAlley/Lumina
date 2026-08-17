#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.CancelLibraryScan;

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
    /// Cancels a running scan of a media library.
    /// </summary>
    /// <param name="request">The request containing the Ids of the media library and of the scan to cancel.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CancelLibraryScanRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PostAsync<Web.Common.Requests.Common.EmptyRequest, Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest>(ApiRoutes.Libraries.CANCEL_LIBRARY_SCAN.Replace("{libraryId}", request.LibraryId.ToString()).Replace("{scanId}", request.ScanId.ToString()), new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest(), cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
