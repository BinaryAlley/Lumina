#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Themes;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the metadata persisted alongside an installed theme.
/// </summary>
[DebuggerDisplay("Source: {Source}")]
public sealed class ThemeInstallationMetadataDto
{
    /// <summary>
    /// Gets or sets the source the theme was installed from.
    /// </summary>
    public ThemeInstallSource Source { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp at which the theme was installed.
    /// </summary>
    public DateTimeOffset InstalledAtUtc { get; set; }
}
