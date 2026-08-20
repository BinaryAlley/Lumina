#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Themes;

/// <summary>
/// Represents a theme response.
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
[DebuggerDisplay("ThemeId: {ThemeId}, Name: {Name}")]
public record ThemeResponse(
    Guid Id,
    string ThemeId,
    string Name,
    string Description,
    string Author,
    string Version,
    string? PreviewPath,
    ThemeInstallSource InstallSource,
    bool? IsCurrent,
    DateTime InstalledAtUtc
);
