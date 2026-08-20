#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Themes;

/// <summary>
/// Represents a request to set the currently active theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the theme to activate.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record SetCurrentThemeRequest(
    string? ThemeId
);
