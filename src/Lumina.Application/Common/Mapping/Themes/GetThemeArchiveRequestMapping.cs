#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;
using Lumina.Contracts.Requests.Themes;
#endregion

namespace Lumina.Application.Common.Mapping.Themes;

/// <summary>
/// Extension methods for converting <see cref="GetThemeArchiveRequest"/>.
/// </summary>
public static class GetThemeArchiveRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetThemeArchiveQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetThemeArchiveQuery ToQuery(this GetThemeArchiveRequest request)
    {
        return new GetThemeArchiveQuery(request.ThemeId);
    }
}
