#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Themes;
using Nemera.Application.Core.Themes.Management.Commands.SetCurrentTheme;
using Nemera.Contracts.Requests.Themes;
using Nemera.Contracts.Responses.Themes;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Themes;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Themes.Management.SetCurrentTheme;

/// <summary>
/// API endpoint for the <c>/themes/current</c> route.
/// </summary>
public class SetCurrentThemeEndpoint : BaseEndpoint<SetCurrentThemeRequest, IResult>
{
    private readonly ICommandHandler<SetCurrentThemeCommand, Result<ThemeResponse>> _setCurrentThemeCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeEndpoint"/> class.
    /// </summary>
    /// <param name="setCurrentThemeCommandHandler">Injected service for handling set current theme commands.</param>
    public SetCurrentThemeEndpoint(ICommandHandler<SetCurrentThemeCommand, Result<ThemeResponse>> setCurrentThemeCommandHandler)
    {
        _setCurrentThemeCommandHandler = setCurrentThemeCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Themes.SET_CURRENT_THEME);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Sets the theme stored in <paramref name="request"/> as the currently active theme.
    /// </summary>
    /// <param name="request">The request containing the theme to activate.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(SetCurrentThemeRequest request, CancellationToken cancellationToken)
    {
        Result<ThemeResponse> result = await _setCurrentThemeCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
