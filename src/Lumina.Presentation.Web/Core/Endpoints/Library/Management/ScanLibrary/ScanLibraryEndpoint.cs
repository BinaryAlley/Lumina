#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.ScanLibrary;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-scan-library/{id}</c> route.
/// </summary>
public class ScanLibraryEndpoint : BaseEndpoint<ScanLibraryRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ScanLibraryEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryManagement.SCAN_LIBRARY);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Initiates the scan of a media library.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library to scan.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ScanLibraryRequest request, CancellationToken cancellationToken)
    {
        ScanLibraryDto response = await _apiHttpClient.PostAsync<ScanLibraryDto, Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest>(ApiRoutes.Libraries.SCAN_LIBRARY.Replace("{id}", request.Id.ToString()), new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest(), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
