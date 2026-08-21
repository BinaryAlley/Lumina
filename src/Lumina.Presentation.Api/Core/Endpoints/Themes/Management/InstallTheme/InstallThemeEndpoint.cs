#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Themes.Management.Commands.InstallTheme;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Management.InstallTheme;

/// <summary>
/// API endpoint for the <c>/themes</c> route.
/// </summary>
public class InstallThemeEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<InstallThemeCommand, Result<ThemeResponse>> _installThemeCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeEndpoint"/> class.
    /// </summary>
    /// <param name="installThemeCommandHandler">Injected service for handling install theme commands.</param>
    public InstallThemeEndpoint(ICommandHandler<InstallThemeCommand, Result<ThemeResponse>> installThemeCommandHandler)
    {
        _installThemeCommandHandler = installThemeCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Themes.INSTALL_THEME);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Installs the theme pack uploaded in the multipart form of the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        IFormFile? archive = HttpContext.Request.Form.Files.FirstOrDefault();
        InstallThemeCommand command = new(archive?.OpenReadStream(), archive?.FileName);
        Result<ThemeResponse> result = await _installThemeCommandHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
