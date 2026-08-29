#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a menu entry of the themed navigation menu, either a link or a submenu.
/// </summary>
/// <param name="Label">The localized label of the menu entry.</param>
/// <param name="Url">The URL of the link, or <see langword="null"/> for a submenu.</param>
/// <param name="CssClass">The CSS classes of the link, such as <c>nav-link</c> for AJAX navigation.</param>
/// <param name="Children">The child links of a submenu, empty for a plain link.</param>
[DebuggerDisplay("Label: {Label}")]
public sealed record ThemeNavEntryDto(
    string Label,
    string? Url,
    string? CssClass,
    IReadOnlyList<ThemeNavEntryDto> Children
)
{
    /// <summary>
    /// Gets whether this entry is a submenu: it has child links instead of a direct URL.
    /// </summary>
    public bool IsSubmenu => Children.Count > 0;
}
