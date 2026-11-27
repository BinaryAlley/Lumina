#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.Themes.Management.Queries.GetCurrentTheme;
using Nemera.Contracts.Responses.Themes;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Themes;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Themes.Queries.GetCurrentTheme;

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
        // Theme content is public, since the web renders themed pages for anonymous visitors too (i.e.: login page); only install and manage operations are admin-gated.
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the currently active theme.
    /// </summary>
    /// <param name="_">The empty request, which carries no data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<ThemeResponse> result = await _getCurrentThemeQueryHandler.HandleAsync(new GetCurrentThemeQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
