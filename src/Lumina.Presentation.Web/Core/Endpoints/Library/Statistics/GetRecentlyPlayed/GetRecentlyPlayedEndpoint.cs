#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetRecentlyPlayed;

/// <summary>
/// API endpoint for the <c>/{culture}/library/statistics/api-get-recently-played</c> route.
/// </summary>
public class GetRecentlyPlayedEndpoint : BaseEndpoint<GetRecentlyPlayedRequest, IResult>
{
    private const int DEFAULT_PAGE_SIZE = 10;

    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRecentlyPlayedEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetRecentlyPlayedEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryStatistics.GET_RECENTLY_PLAYED);
        DontAutoTag();
        Options(options => options.WithTags("Statistics"));
    }

    /// <summary>
    /// Retrieves a page of the recently played tracks of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetRecentlyPlayedRequest request, CancellationToken cancellationToken)
    {
        // The statistics belong to the authenticated user, so no intermediary may store them, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The query string is rebuilt from the typed request values instead of forwarding the raw one, so that crafted text can never alter the upstream URL.
        int currentPage = request.CurrentPage ?? 1;
        int perPage = request.PerPage ?? DEFAULT_PAGE_SIZE;
        string url = ApiRoutes.PlayStatistics.GET_RECENTLY_PLAYED + $"?currentPage={currentPage.ToString(CultureInfo.InvariantCulture)}&perPage={perPage.ToString(CultureInfo.InvariantCulture)}";
        PaginatedDto<PlayedTrackDto> recentlyPlayed = await _apiHttpClient.GetAsync<PaginatedDto<PlayedTrackDto>>(url, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(recentlyPlayed);
    }
}
