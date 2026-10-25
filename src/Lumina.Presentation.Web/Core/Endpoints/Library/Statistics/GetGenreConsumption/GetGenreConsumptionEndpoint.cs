#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetGenreConsumption;

/// <summary>
/// API endpoint for the <c>/{culture}/library/statistics/api-get-genre-consumption</c> route.
/// </summary>
public class GetGenreConsumptionEndpoint : BaseEndpoint<GetGenreConsumptionRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetGenreConsumptionEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryStatistics.GET_GENRE_CONSUMPTION);
        DontAutoTag();
        Options(options => options.WithTags("Statistics"));
    }

    /// <summary>
    /// Retrieves the genre consumption of the selected user.
    /// </summary>
    /// <param name="request">The request containing the selected user and the optional filters.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetGenreConsumptionRequest request, CancellationToken cancellationToken)
    {
        // The statistics belong to the selected user, so no intermediary may store them, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        GenreConsumptionDto[] response = await _apiHttpClient.GetAsync<GenreConsumptionDto[]>(BuildGenreConsumptionEndpoint(request), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }

    /// <summary>
    /// Builds the API endpoint used to retrieve the genre consumption of the selected user.
    /// </summary>
    /// <param name="request">The request containing the selected user and the optional filters.</param>
    /// <returns>The API endpoint to which the genre request is sent.</returns>
    private static string BuildGenreConsumptionEndpoint(GetGenreConsumptionRequest request)
    {
        // The query string is rebuilt from the typed request values instead of forwarding the raw one, so that crafted text can never alter the upstream URL.
        StringBuilder endpoint = new(string.IsNullOrWhiteSpace(request.UserId)
            ? ApiRoutes.LibraryStatistics.GET_GENRE_CONSUMPTION
            : ApiRoutes.LibraryStatistics.GET_USER_GENRE_CONSUMPTION.Replace("{userId}", Uri.EscapeDataString(request.UserId), StringComparison.Ordinal));
        List<string> parameters = [];
        if (!string.IsNullOrWhiteSpace(request.LibraryId))
            parameters.Add($"libraryId={Uri.EscapeDataString(request.LibraryId)}");
        if (request.MediaKind is not null)
            parameters.Add($"mediaKind={Uri.EscapeDataString(request.MediaKind.Value.ToString())}");
        if (request.FromUtc is not null)
            parameters.Add($"fromUtc={Uri.EscapeDataString(request.FromUtc.Value.ToString("O", CultureInfo.InvariantCulture))}");
        if (request.ToUtc is not null)
            parameters.Add($"toUtc={Uri.EscapeDataString(request.ToUtc.Value.ToString("O", CultureInfo.InvariantCulture))}");
        if (parameters.Count > 0)
            endpoint.Append('?').Append(string.Join('&', parameters));
        return endpoint.ToString();
    }
}
