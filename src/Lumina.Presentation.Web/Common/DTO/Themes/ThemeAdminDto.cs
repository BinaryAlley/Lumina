#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the data shown on the theme administration page.
/// </summary>
[DebuggerDisplay("CurrentThemeId: {CurrentThemeId}")]
public sealed class ThemeAdminDto
{
    /// <summary>
    /// Gets or sets the metadata of all installed themes.
    /// </summary>
    public required IReadOnlyList<ThemeInfoDto> Themes { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier of the currently selected theme.
    /// </summary>
    public required string CurrentThemeId { get; init; }

    /// <summary>
    /// Gets or sets the maximum allowed size of a theme archive, in bytes.
    /// </summary>
    public required long MaxArchiveBytes { get; init; }

    /// <summary>
    /// Gets the maximum allowed size of a theme archive, in megabytes.
    /// </summary>
    public double MaxArchiveMegabytes => Math.Round(MaxArchiveBytes / 1024d / 1024d, 1);
}
