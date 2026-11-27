#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.UsersManagement;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Tools.Settings.UpdatePlaybackState;

/// <summary>
/// API endpoint for the <c>/{culture}/tools/settings/api-update-playback-state</c> route.
/// </summary>
public class UpdatePlaybackStateEndpoint : BaseEndpoint<PlaybackStateDto, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdatePlaybackStateEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Settings.UPDATE_PLAYBACK_STATE);
        DontAutoTag();
        Options(options => options.WithTags("Settings"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the playback state the player reported for the current user automatically.
    /// </summary>
    /// <param name="request">The request containing the latest playback state of the current user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(PlaybackStateDto request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutIgnoringResponseAsync<PlaybackStateDto>(ApiRoutes.Users.UPDATE_PLAYBACK_STATE, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { isUpdated = true });
    }
}
