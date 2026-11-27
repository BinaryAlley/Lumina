#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Themes.Management.Commands.DeleteTheme;
using Nemera.Contracts.Requests.Themes;
#endregion

namespace Nemera.Application.Common.Mapping.Themes;

/// <summary>
/// Extension methods for converting <see cref="DeleteThemeRequest"/>.
/// </summary>
public static class DeleteThemeRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="DeleteThemeCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static DeleteThemeCommand ToCommand(this DeleteThemeRequest request)
    {
        return new DeleteThemeCommand(request.ThemeId);
    }
}
