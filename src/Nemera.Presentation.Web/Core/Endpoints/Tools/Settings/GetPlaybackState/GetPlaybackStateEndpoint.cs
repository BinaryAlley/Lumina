#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.UsersManagement;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Tools.Settings.GetPlaybackState;

/// <summary>
/// API endpoint for the <c>/{culture}/tools/settings/api-get-playback-state</c> route.
/// </summary>
public class GetPlaybackStateEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetPlaybackStateEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Settings.GET_PLAYBACK_STATE);
        DontAutoTag();
        Options(options => options.WithTags("Settings"));
    }

    /// <summary>
    /// Retrieves the playback state the player reported for the current user automatically.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        PlaybackStateDto response = await _apiHttpClient.GetAsync<PlaybackStateDto>(ApiRoutes.Users.GET_PLAYBACK_STATE, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
