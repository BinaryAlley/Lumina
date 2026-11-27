#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Lumina.Presentation.Web.Core.Themes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.InstallTheme;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/themes/api-install-theme</c> route.
/// </summary>
public class InstallThemeEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly ThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeEndpoint"/> class.
    /// </summary>
    /// <param name="themeService">Injected service for interactions with the remote theme endpoints.</param>
    public InstallThemeEndpoint(ThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.Themes.INSTALL_THEME);
        DontAutoTag();
        Options(options => options.WithTags("Themes"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Installs the theme pack uploaded in the multipart form of the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        IFormFile? archive = HttpContext.Request.Form.Files.FirstOrDefault();
        if (archive is null)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "The uploaded theme archive is missing.");

        await using Stream archiveStream = archive.OpenReadStream();
        ThemeInfoDto theme = await _themeService.InstallAsync(archiveStream, archive.FileName, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(theme);
    }
}
