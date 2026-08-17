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

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.UpdateUserSettings;

/// <summary>
/// API endpoint for the <c>/{culture}/tools/settings/api-update-user-settings</c> route.
/// </summary>
public class UpdateUserSettingsEndpoint : BaseEndpoint<UserSettingsDto, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdateUserSettingsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Settings.UPDATE_USER_SETTINGS);
        DontAutoTag();
        Options(options => options.WithTags("Settings"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the settings of the current user.
    /// </summary>
    /// <param name="request">The request containing the updated settings of the current user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UserSettingsDto request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutAsync<Web.Common.Requests.Common.EmptyRequest, UserSettingsDto>(ApiRoutes.Users.UPDATE_USER_SETTINGS, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { isUpdated = true });
    }
}
