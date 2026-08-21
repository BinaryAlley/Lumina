#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Requests.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.SetCurrentTheme;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/themes/api-set-current-theme</c> route.
/// </summary>
public class SetCurrentThemeEndpoint : BaseEndpoint<SetCurrentThemeRequest, IResult>
{
    private readonly ThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeEndpoint"/> class.
    /// </summary>
    /// <param name="themeService">Injected service for interactions with the remote theme endpoints.</param>
    public SetCurrentThemeEndpoint(ThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.Themes.SET_CURRENT_THEME);
        DontAutoTag();
        Options(options => options.WithTags("Themes"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Sets the theme identified by <paramref name="request"/> as the currently active theme.
    /// </summary>
    /// <param name="request">The request containing the theme to activate.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SetCurrentThemeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ThemeId))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "The theme id cannot be empty.");

        await _themeService.SetCurrentThemeAsync(request.ThemeId, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
