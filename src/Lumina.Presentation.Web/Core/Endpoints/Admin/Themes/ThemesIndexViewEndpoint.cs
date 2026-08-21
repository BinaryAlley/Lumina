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

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/manage-themes</c> route.
/// </summary>
public class ThemesIndexViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly ThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemesIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="themeService">Injected service for the theme operations of the remote API.</param>
    public ThemesIndexViewEndpoint(ThemeService themeService)
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
        Routes(WebRoutes.Admin.MANAGE_THEMES);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Displays the themes management view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ThemeInfoDto> themes = await _themeService.GetThemesAsync(cancellationToken).ConfigureAwait(false);
        ThemeInfoDto currentTheme = await _themeService.GetCurrentThemeAsync(cancellationToken).ConfigureAwait(false);

        Dictionary<string, object?> viewData = new()
        {
            ["themes"] = themes,
            ["currentThemeId"] = currentTheme.Id
        };
        return View("/Core/Views/Admin/Themes.cshtml", viewData: viewData);
    }
}
