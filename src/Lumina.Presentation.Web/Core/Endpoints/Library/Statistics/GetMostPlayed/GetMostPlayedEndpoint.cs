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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetMostPlayed;

/// <summary>
/// API endpoint for the <c>/{culture}/library/statistics/api-get-most-played</c> route.
/// </summary>
public class GetMostPlayedEndpoint : BaseEndpoint<GetMostPlayedRequest, IResult>
{
    private const int DEFAULT_PAGE_SIZE = 10;

    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostPlayedEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetMostPlayedEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryStatistics.GET_MOST_PLAYED);
        DontAutoTag();
        Options(options => options.WithTags("Statistics"));
    }

    /// <summary>
    /// Retrieves a page of the most played tracks of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMostPlayedRequest request, CancellationToken cancellationToken)
    {
        // The statistics belong to the authenticated user, so no intermediary may store them, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The query string is rebuilt from the typed request values instead of forwarding the raw one, so that crafted text can never alter the upstream URL.
        int currentPage = request.CurrentPage ?? 1;
        int perPage = request.PerPage ?? DEFAULT_PAGE_SIZE;
        string url = ApiRoutes.PlayStatistics.GET_MOST_PLAYED + $"?currentPage={currentPage.ToString(CultureInfo.InvariantCulture)}&perPage={perPage.ToString(CultureInfo.InvariantCulture)}";
        PaginatedDto<PlayedTrackDto> mostPlayed = await _apiHttpClient.GetAsync<PaginatedDto<PlayedTrackDto>>(url, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(mostPlayed);
    }
}
