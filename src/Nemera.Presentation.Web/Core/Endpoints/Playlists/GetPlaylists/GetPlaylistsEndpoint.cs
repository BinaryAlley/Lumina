#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.Playlists;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/api-get-playlists</c> route.
/// </summary>
public class GetPlaylistsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetPlaylistsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Playlists.GET_PLAYLISTS);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
    }

    /// <summary>
    /// Retrieves the playlists of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The playlists belong to the authenticated user, so no intermediary may store them, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        PlaylistSummaryDto[] response = await _apiHttpClient.GetAsync<PlaylistSummaryDto[]>(ApiRoutes.Playlists.GET_PLAYLISTS, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
