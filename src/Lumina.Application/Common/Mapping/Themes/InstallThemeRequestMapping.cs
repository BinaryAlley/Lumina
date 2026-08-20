#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Commands.InstallTheme;
using Lumina.Contracts.Requests.Themes;
using System.IO;
#endregion

namespace Lumina.Application.Common.Mapping.Themes;

/// <summary>
/// Extension methods for converting <see cref="InstallThemeRequest"/>.
/// </summary>
public static class InstallThemeRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="InstallThemeCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="archive">The ZIP archive stream of the theme pack.</param>
    /// <param name="fileName">The file name of the uploaded archive.</param>
    /// <returns>The converted command.</returns>
    public static InstallThemeCommand ToCommand(this InstallThemeRequest request, Stream? archive, string? fileName)
    {
        return new InstallThemeCommand(archive, fileName);
    }
}
