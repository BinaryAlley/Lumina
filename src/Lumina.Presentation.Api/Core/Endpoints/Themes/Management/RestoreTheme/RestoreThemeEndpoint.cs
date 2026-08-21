#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Commands.RestoreTheme;
using Lumina.Contracts.Requests.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Management.RestoreTheme;

/// <summary>
/// API endpoint for the <c>/themes/{themeId}/restore</c> route.
/// </summary>
public class RestoreThemeEndpoint : BaseEndpoint<RestoreThemeRequest, IResult>
{
    private readonly ICommandHandler<RestoreThemeCommand, Result<Success>> _restoreThemeCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeEndpoint"/> class.
    /// </summary>
    /// <param name="restoreThemeCommandHandler">Injected service for handling restore theme commands.</param>
    public RestoreThemeEndpoint(ICommandHandler<RestoreThemeCommand, Result<Success>> restoreThemeCommandHandler)
    {
        _restoreThemeCommandHandler = restoreThemeCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Themes.RESTORE_THEME);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Restores the soft deleted bundled theme identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the theme to restore.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(RestoreThemeRequest request, CancellationToken cancellationToken)
    {
        Result<Success> result = await _restoreThemeCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.NoContent(), Problem);
    }
}
