#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Contracts.Responses.Themes;
#endregion

namespace Lumina.Application.Common.Mapping.Themes;

/// <summary>
/// Extension methods for converting <see cref="ThemeEntity"/>.
/// </summary>
public static class ThemeEntityMapping
{
    /// <summary>
    /// Converts <paramref name="theme"/> to <see cref="ThemeResponse"/>.
    /// </summary>
    /// <param name="theme">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ThemeResponse ToResponse(this ThemeEntity theme)
    {
        return new ThemeResponse(
            theme.Id,
            theme.ThemeId,
            theme.Name,
            theme.Description,
            theme.Author,
            theme.Version,
            theme.PreviewPath,
            theme.InstallSource,
            theme.IsCurrent,
            theme.InstalledAtUtc,
            theme.IsDeleted);
    }
}
