#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.Requests.Themes;

/// <summary>
/// Represents a request to restore a soft deleted bundled theme.
/// </summary>
/// <param name="ThemeId">The manifest id of the bundled theme to restore. Required.</param>
[DebuggerDisplay("ThemeId: {ThemeId}")]
public record RestoreThemeRequest(
    string? ThemeId
);
