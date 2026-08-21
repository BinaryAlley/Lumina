#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeSettings;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeSettings;

/// <summary>
/// API endpoint for the <c>/themes/settings</c> route.
/// </summary>
public class GetThemeSettingsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetThemeSettingsQuery, Result<ThemeSettingsResponse>> _getThemeSettingsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="getThemeSettingsQueryHandler">Injected service for handling get theme settings queries.</param>
    public GetThemeSettingsEndpoint(IQueryHandler<GetThemeSettingsQuery, Result<ThemeSettingsResponse>> getThemeSettingsQueryHandler)
    {
        _getThemeSettingsQueryHandler = getThemeSettingsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Themes.GET_THEME_SETTINGS);
        Version(1);
        // theme content is public, since the web renders themed pages for anonymous visitors too (i.e.: login page); only install and manage operations are admin-gated
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the theme engine settings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<ThemeSettingsResponse> result = await _getThemeSettingsQueryHandler.HandleAsync(new GetThemeSettingsQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
