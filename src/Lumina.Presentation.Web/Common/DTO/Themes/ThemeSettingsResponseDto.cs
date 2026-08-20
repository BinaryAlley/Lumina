#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the theme engine settings returned by the remote API.
/// </summary>
/// <param name="MaxArchiveBytes">The maximum allowed size of a theme archive, in bytes.</param>
/// <param name="AllowThemeScripts">Whether theme templates may contain script elements and theme assets may include script files.</param>
/// <param name="DefaultThemeId">The identifier of the theme selected when no valid current theme is available.</param>
[DebuggerDisplay("MaxArchiveBytes: {MaxArchiveBytes}")]
public sealed record ThemeSettingsResponseDto(
    long MaxArchiveBytes,
    bool AllowThemeScripts,
    string DefaultThemeId
);
