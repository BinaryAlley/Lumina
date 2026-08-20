#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Themes;

/// <summary>
/// Represents a theme settings response.
/// </summary>
/// <param name="MaxArchiveBytes">The maximum allowed size of a theme archive, in bytes.</param>
/// <param name="AllowThemeScripts">Whether theme templates may contain script elements and theme assets may include script files.</param>
/// <param name="DefaultThemeId">The identifier of the theme selected when no valid current theme is available.</param>
[DebuggerDisplay("MaxArchiveBytes: {MaxArchiveBytes}")]
public record ThemeSettingsResponse(
    long MaxArchiveBytes,
    bool AllowThemeScripts,
    string DefaultThemeId
);
