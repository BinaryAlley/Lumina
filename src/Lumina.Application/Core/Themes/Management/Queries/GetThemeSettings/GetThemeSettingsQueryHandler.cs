#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemeSettings;

/// <summary>
/// Handler for the query to get the theme engine settings.
/// </summary>
public class GetThemeSettingsQueryHandler : IQueryHandler<GetThemeSettingsQuery, Result<ThemeSettingsResponse>>
{
    private readonly IThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeSettingsQueryHandler"/> class.
    /// </summary>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    public GetThemeSettingsQueryHandler(IThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Handles the query to get the theme engine settings.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing the theme engine settings, or an error.
    /// </returns>
    public Task<Result<ThemeSettingsResponse>> HandleAsync(GetThemeSettingsQuery query, CancellationToken cancellationToken)
    {
        ThemeSettingsResponse response = new(_themeService.MaxArchiveBytes, _themeService.AllowThemeScripts, _themeService.DefaultThemeId);
        return Task.FromResult(Result.From(response));
    }
}
