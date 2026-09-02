#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Themes;

/// <summary>
/// Represents a request to get an asset file of a theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme. Required.</param>
/// <param name="Path">The asset path relative to the theme pack root. Required.</param>
[DebuggerDisplay("ThemeId: {ThemeId}, Path: {Path}")]
public record GetThemeAssetRequest(
    string? ThemeId,
    string? Path
);
