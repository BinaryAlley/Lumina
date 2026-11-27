#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.GetThemes;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/themes/api-get-themes</c> route.
/// </summary>
public class GetThemesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly ThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesEndpoint"/> class.
    /// </summary>
    /// <param name="themeService">Injected service for interactions with the remote theme endpoints.</param>
    public GetThemesEndpoint(ThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Themes.GET_THEMES);
        DontAutoTag();
        Options(options => options.WithTags("Themes"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Gets the installed themes together with the current theme and the theme engine settings.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ThemeInfoDto> themes = await _themeService.GetThemesAsync(cancellationToken).ConfigureAwait(false);
        ThemeInfoDto currentTheme = await _themeService.GetCurrentThemeAsync(cancellationToken).ConfigureAwait(false);
        ThemeSettingsResponseDto settings = await _themeService.GetThemeSettingsAsync(cancellationToken).ConfigureAwait(false);

        ThemeAdminDto themeAdmin = new()
        {
            Themes = themes,
            CurrentThemeId = currentTheme.Id,
            MaxArchiveBytes = settings.MaxArchiveBytes
        };
        return JsonSuccess(themeAdmin);
    }
}
