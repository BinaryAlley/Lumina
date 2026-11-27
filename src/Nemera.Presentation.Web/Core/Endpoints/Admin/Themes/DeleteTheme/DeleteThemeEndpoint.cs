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

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.DeleteTheme;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/themes/api-delete-theme/{themeId}</c> route.
/// </summary>
public class DeleteThemeEndpoint : BaseEndpoint<DeleteThemeRequest, IResult>
{
    private readonly ThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteThemeEndpoint"/> class.
    /// </summary>
    /// <param name="themeService">Injected service for interactions with the remote theme endpoints.</param>
    public DeleteThemeEndpoint(ThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.DELETE);
        Routes(WebRoutes.Themes.DELETE_THEME);
        DontAutoTag();
        Options(options => options.WithTags("Themes"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes the theme identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the theme to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteThemeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ThemeId))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "The theme id cannot be empty.");

        await _themeService.DeleteThemeAsync(request.ThemeId, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
