#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a menu section of the themed navigation menu.
/// </summary>
/// <param name="Label">The localized label of the menu section.</param>
/// <param name="Items">The menu entries of the section, visible to the current user.</param>
[DebuggerDisplay("Label: {Label}")]
public sealed record ThemeNavSectionDto(
    string Label,
    IReadOnlyList<ThemeNavEntryDto> Items
);
