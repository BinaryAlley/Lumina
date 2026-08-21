#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Themes;

/// <summary>
/// Represents a request to download the archive of a theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme to download.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record GetThemeArchiveRequest(
    string? ThemeId
);
