#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the model available to the themed navigation menu template.
/// </summary>
/// <param name="SiteName">The name of the site displayed by the navigation menu.</param>
/// <param name="MobileSections">The menu sections of the mobile navigation menu.</param>
/// <param name="MenubarSections">The menu sections of the desktop navigation menu bar.</param>
[DebuggerDisplay("SiteName: {SiteName}")]
public sealed record ThemeNavMenuDto(
    string SiteName,
    IReadOnlyList<ThemeNavSectionDto> MobileSections,
    IReadOnlyList<ThemeNavSectionDto> MenubarSections
);
