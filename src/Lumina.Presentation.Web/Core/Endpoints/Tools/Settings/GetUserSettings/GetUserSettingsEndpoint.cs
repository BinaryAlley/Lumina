#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.GetUserSettings;

/// <summary>
/// API endpoint for the <c>/{culture}/tools/settings/api-get-user-settings</c> route.
/// </summary>
public class GetUserSettingsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetUserSettingsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Settings.GET_USER_SETTINGS);
        DontAutoTag();
        Options(options => options.WithTags("Settings"));
    }

    /// <summary>
    /// Retrieves the settings of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        UserSettingsDto response = await _apiHttpClient.GetAsync<UserSettingsDto>(ApiRoutes.Users.GET_USER_SETTINGS, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
