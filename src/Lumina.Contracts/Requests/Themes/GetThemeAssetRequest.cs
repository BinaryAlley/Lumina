#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Themes;

/// <summary>
/// Represents a request to get an asset file of a theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme. Required.</param>
/// <param name="AssetPath">The asset path relative to the theme pack root. Required.</param>
[DebuggerDisplay("ThemeId: {ThemeId}, AssetPath: {AssetPath}")]
public record GetThemeAssetRequest(
    string? ThemeId,
    string? AssetPath
);
