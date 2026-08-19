#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for an installed theme.
/// </summary>
/// <param name="Manifest">The validated theme manifest.</param>
/// <param name="Info">The display metadata of the theme.</param>
/// <param name="RootPath">The absolute path of the installed theme directory.</param>
[DebuggerDisplay("Manifest name: {Manifest.Name}")]
public sealed record InstalledThemeDto(
    ThemeManifestDto Manifest,
    ThemeInfoDto Info,
    string RootPath
);
