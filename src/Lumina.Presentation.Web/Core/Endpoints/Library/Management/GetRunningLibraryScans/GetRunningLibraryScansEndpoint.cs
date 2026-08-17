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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetRunningLibraryScans;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage/api-get-running-library-scans</c> route.
/// </summary>
public class GetRunningLibraryScansEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetRunningLibraryScansEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.LibraryManagement.GET_RUNNING_LIBRARY_SCANS);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
    }

    /// <summary>
    /// Retrieves the running media library scans.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        LibraryScanProgressDto[] response = await _apiHttpClient.GetAsync<LibraryScanProgressDto[]>(ApiRoutes.Libraries.GET_RUNNING_LIBRARIES_SCAN, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
