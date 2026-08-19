#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a navigation entry rendered by a theme.
/// </summary>
/// <param name="Key">The unique key of the navigation entry.</param>
/// <param name="Label">The display label of the navigation entry.</param>
/// <param name="Url">The URL the navigation entry links to.</param>
/// <param name="IsCurrent">Whether the navigation entry represents the currently displayed page.</param>
[DebuggerDisplay("Key: {Key}, Label: {Label}")]
public sealed record NavigationItemDto(
    string Key,
    string Label,
    string Url,
    bool IsCurrent
);
