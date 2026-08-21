#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Themes;

/// <summary>
/// Represents a request to restore a soft deleted bundled theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the bundled theme to restore.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record RestoreThemeRequest(
    string? ThemeId
);
