#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Themes;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a theme returned by the remote API.
/// </summary>
/// <param name="Id">The unique identifier of the theme.</param>
/// <param name="ThemeId">The manifest id of the theme, used by clients to reference it.</param>
/// <param name="Name">The display name of the theme.</param>
/// <param name="Description">The description of the theme.</param>
/// <param name="Author">The author of the theme.</param>
/// <param name="Version">The version of the theme, using semantic version form.</param>
/// <param name="PreviewPath">The path of the theme preview image, relative to the theme pack root, or <see langword="null"/> when the theme has no preview.</param>
/// <param name="InstallSource">The source the theme was installed from.</param>
/// <param name="IsCurrent">Whether the theme is the currently active one.</param>
/// <param name="InstalledAtUtc">The UTC timestamp at which the theme was installed.</param>
/// <param name="IsDeleted">Whether the theme was deleted by the user, which is only possible for bundled themes.</param>
[DebuggerDisplay("ThemeId: {ThemeId}, Name: {Name}")]
public sealed record ThemeResponseDto(
    Guid Id,
    string ThemeId,
    string Name,
    string Description,
    string Author,
    string Version,
    string? PreviewPath,
    ThemeInstallSource InstallSource,
    bool? IsCurrent,
    DateTime InstalledAtUtc,
    bool IsDeleted
);
