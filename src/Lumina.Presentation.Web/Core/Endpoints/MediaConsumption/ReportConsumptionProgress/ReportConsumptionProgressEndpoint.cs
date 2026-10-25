#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;

/// <summary>
/// API endpoint for the <c>/{culture}/media-consumption/api-report-consumption-progress/{recordId}</c> route.
/// </summary>
public class ReportConsumptionProgressEndpoint : BaseEndpoint<ReportConsumptionProgressRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConsumptionProgressEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ReportConsumptionProgressEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.MediaConsumption.REPORT_CONSUMPTION_PROGRESS);
        DontAutoTag();
        Options(options => options.WithTags("MediaConsumption"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Reports the progress of a consumption session of the current user.
    /// </summary>
    /// <param name="request">The request containing the latest position, fraction, and consumed duration of the session.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ReportConsumptionProgressRequest request, CancellationToken cancellationToken)
    {
        // The consumption record belongs to the current user, so no intermediary may store the response, and it varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The record is identified by the route value, not by any id posted in the body.
        string routeRecordId = HttpContext.Request.RouteValues["recordId"]?.ToString() ?? string.Empty;
        string recordId = Guid.TryParse(routeRecordId, out Guid parsedRecordId) ? parsedRecordId.ToString() : Guid.Empty.ToString();
        ConsumptionRecordDto record = await _apiHttpClient
            .PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(
                ApiRoutes.MediaConsumption.REPORT_CONSUMPTION_PROGRESS.Replace("{recordId}", recordId, StringComparison.Ordinal),
                request,
                cancellationToken).ConfigureAwait(false);
        return JsonSuccess(record);
    }
}
