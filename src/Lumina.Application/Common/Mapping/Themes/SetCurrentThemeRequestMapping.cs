#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;
using Lumina.Contracts.Requests.Themes;
#endregion

namespace Lumina.Application.Common.Mapping.Themes;

/// <summary>
/// Extension methods for converting <see cref="SetCurrentThemeRequest"/>.
/// </summary>
public static class SetCurrentThemeRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="SetCurrentThemeCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static SetCurrentThemeCommand ToCommand(this SetCurrentThemeRequest request)
    {
        return new SetCurrentThemeCommand(request.ThemeId);
    }
}
