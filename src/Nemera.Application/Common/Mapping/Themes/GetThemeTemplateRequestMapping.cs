#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Themes.Management.Queries.GetThemeTemplate;
using Nemera.Contracts.Requests.Themes;
#endregion

namespace Nemera.Application.Common.Mapping.Themes;

/// <summary>
/// Extension methods for converting <see cref="GetThemeTemplateRequest"/>.
/// </summary>
public static class GetThemeTemplateRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetThemeTemplateQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetThemeTemplateQuery ToQuery(this GetThemeTemplateRequest request)
    {
        return new GetThemeTemplateQuery(request.ThemeId, request.PageKey);
    }
}
