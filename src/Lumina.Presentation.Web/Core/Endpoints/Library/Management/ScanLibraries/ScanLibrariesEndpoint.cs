#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.ScanLibraries;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-scan-libraries</c> route.
/// </summary>
public class ScanLibrariesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ScanLibrariesEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryManagement.SCAN_LIBRARIES);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Initiates the scan of all the media libraries.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        ScanLibraryDto[] response = await _apiHttpClient.PostAsync<ScanLibraryDto[], Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest>(ApiRoutes.Libraries.SCAN_LIBRARIES, new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest(), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
