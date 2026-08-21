#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
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
    private readonly ThemeCachePreferenceService _themeCachePreferenceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="themeCachePreferenceService">Injected service for storing the per-user theme cache preference.</param>
    public UpdateUserSettingsEndpoint(IApiHttpClient apiHttpClient, ThemeCachePreferenceService themeCachePreferenceService)
    {
        _apiHttpClient = apiHttpClient;
        _themeCachePreferenceService = themeCachePreferenceService;
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

        // keep the per-user theme cache preference in sync with the stored settings, so the handler applies it immediately
        Guid? userId = GetCurrentUserId();
        if (userId is not null)
            await _themeCachePreferenceService.SetAsync(userId.Value, request.IsThemeCachingEnabled, cancellationToken).ConfigureAwait(false);

        return JsonSuccess(new { isUpdated = true });
    }

    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>The unique identifier of the current user, or <see langword="null"/> when the request is anonymous.</returns>
    private Guid? GetCurrentUserId()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out Guid parsedUserId) ? parsedUserId : null;
    }
}
