#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Requests.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.DownloadTheme;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/themes/api-download-theme/{themeId}</c> route.
/// </summary>
public class DownloadThemeEndpoint : BaseEndpoint<GetThemeArchiveRequest, IResult>
{
    private readonly ThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadThemeEndpoint"/> class.
    /// </summary>
    /// <param name="themeService">Injected service for interactions with the remote theme endpoints.</param>
    public DownloadThemeEndpoint(ThemeService themeService)
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
        Routes(WebRoutes.Themes.DOWNLOAD_THEME);
        DontAutoTag();
        Options(options => options.WithTags("Themes"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Downloads the archive of the theme identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the theme to download.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetThemeArchiveRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ThemeId))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "The theme id cannot be empty.");

        ThemeArchiveDto archive = await _themeService.BuildArchiveAsync(request.ThemeId, cancellationToken).ConfigureAwait(false);
        return Results.File(archive.Content, "application/zip", archive.FileName);
    }
}
