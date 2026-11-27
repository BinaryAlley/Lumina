#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Commands.RestoreTheme;
using Lumina.Contracts.Requests.Themes;
#endregion

namespace Lumina.Application.Common.Mapping.Themes;

/// <summary>
/// Extension methods for converting <see cref="RestoreThemeRequest"/>.
/// </summary>
public static class RestoreThemeRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="RestoreThemeCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static RestoreThemeCommand ToCommand(this RestoreThemeRequest request)
    {
        return new RestoreThemeCommand(request.ThemeId);
    }
}
