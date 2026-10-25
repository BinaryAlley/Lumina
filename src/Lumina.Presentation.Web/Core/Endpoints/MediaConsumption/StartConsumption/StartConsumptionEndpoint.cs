#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// API endpoint for the <c>/{culture}/media-consumption/api-start-consumption/{libraryId}/{mediaKind}/{mediaItemId}</c> route.
/// </summary>
public class StartConsumptionEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    // A value that is not a name of MediaKind, so the API rejects a crafted route value with its own validation error.
    private const string INVALID_MEDIA_KIND_PLACEHOLDER = "Invalid";

    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public StartConsumptionEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.MediaConsumption.START_CONSUMPTION);
        DontAutoTag();
        Options(options => options.WithTags("MediaConsumption"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Starts a consumption session of a media item for the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The consumption record belongs to the current user, so no intermediary may store the response, and it varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The identifiers are taken from the route, so they cannot be replaced by values posted in the body.
        // The media kind must be the name of a MediaKind member, which also rejects numeric strings, comma separated lists, and
        // dot segments that escaping alone would leave in the relative path of the upstream request.
        string routeLibraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString() ?? string.Empty;
        string libraryId = Guid.TryParse(routeLibraryId, out Guid parsedLibraryId) ? parsedLibraryId.ToString() : Guid.Empty.ToString();
        string routeMediaKind = HttpContext.Request.RouteValues["mediaKind"]?.ToString() ?? string.Empty;
        string mediaKind = INVALID_MEDIA_KIND_PLACEHOLDER;
        foreach (string mediaKindName in Enum.GetNames<MediaKind>())
            if (string.Equals(mediaKindName, routeMediaKind, StringComparison.OrdinalIgnoreCase))
                mediaKind = mediaKindName;
        string routeMediaItemId = HttpContext.Request.RouteValues["mediaItemId"]?.ToString() ?? string.Empty;
        string mediaItemId = Guid.TryParse(routeMediaItemId, out Guid parsedMediaItemId) ? parsedMediaItemId.ToString() : Guid.Empty.ToString();
        string endpoint = ApiRoutes.MediaConsumption.START_CONSUMPTION
            .Replace("{libraryId}", libraryId, StringComparison.Ordinal)
            .Replace("{mediaKind}", mediaKind, StringComparison.Ordinal)
            .Replace("{mediaItemId}", mediaItemId, StringComparison.Ordinal);
        ConsumptionRecordDto record = await _apiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(endpoint, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(record);
    }
}
