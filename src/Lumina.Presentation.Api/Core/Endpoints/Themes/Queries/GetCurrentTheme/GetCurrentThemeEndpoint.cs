#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Themes.Management.Queries.GetCurrentTheme;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetCurrentTheme;

/// <summary>
/// API endpoint for the <c>/themes/current</c> route.
/// </summary>
public class GetCurrentThemeEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetCurrentThemeQuery, Result<ThemeResponse>> _getCurrentThemeQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentThemeEndpoint"/> class.
    /// </summary>
    /// <param name="getCurrentThemeQueryHandler">Injected service for handling get current theme queries.</param>
    public GetCurrentThemeEndpoint(IQueryHandler<GetCurrentThemeQuery, Result<ThemeResponse>> getCurrentThemeQueryHandler)
    {
        _getCurrentThemeQueryHandler = getCurrentThemeQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Themes.GET_CURRENT_THEME);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the currently active theme.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<ThemeResponse> result = await _getCurrentThemeQueryHandler.HandleAsync(new GetCurrentThemeQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
