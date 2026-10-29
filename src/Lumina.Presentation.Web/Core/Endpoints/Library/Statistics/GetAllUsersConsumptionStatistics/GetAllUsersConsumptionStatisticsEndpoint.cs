#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// API endpoint for the <c>/{culture}/library/statistics/api-get-all-users-statistics</c> route.
/// </summary>
public class GetAllUsersConsumptionStatisticsEndpoint : BaseEndpoint<GetAllUsersConsumptionStatisticsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetAllUsersConsumptionStatisticsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryStatistics.GET_ALL_USERS_STATISTICS);
        DontAutoTag();
        Options(options => options.WithTags("Statistics"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Retrieves the combined consumption statistics of every user.
    /// </summary>
    /// <param name="request">The request containing the optional filters.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetAllUsersConsumptionStatisticsRequest request, CancellationToken cancellationToken)
    {
        // The statistics belong to every user of the server, so no intermediary may store them, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        AllUsersConsumptionStatisticsDto response = await _apiHttpClient.GetAsync<AllUsersConsumptionStatisticsDto>(BuildStatisticsEndpoint(request), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }

    /// <summary>
    /// Builds the API endpoint used to retrieve the combined consumption statistics of every user.
    /// </summary>
    /// <param name="request">The request containing the optional filters.</param>
    /// <returns>The API endpoint to which the statistics request is sent.</returns>
    private static string BuildStatisticsEndpoint(GetAllUsersConsumptionStatisticsRequest request)
    {
        // The query string is rebuilt from the typed request values instead of forwarding the raw one, so that crafted text can never alter the upstream URL.
        StringBuilder endpoint = new(ApiRoutes.LibraryStatistics.GET_ALL_USERS_CONSUMPTION_STATISTICS);
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
