#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for a content entry rendered by a theme.
/// </summary>
/// <param name="Number">The ordering number of the content entry.</param>
/// <param name="Title">The title of the content entry.</param>
/// <param name="Description">The description of the content entry.</param>
/// <param name="Meta">The metadata line of the content entry.</param>
/// <param name="Badge">The badge text of the content entry.</param>
/// <param name="Accent">The accent color of the content entry.</param>
/// <param name="Glyph">The glyph or icon of the content entry.</param>
/// <param name="Url">The URL the content entry links to.</param>
[DebuggerDisplay("Number: {Number}, Title: {Title}")]
public sealed record ContentItemDto(
    string Number,
    string Title,
    string Description,
    string Meta,
    string Badge,
    string Accent,
    string Glyph,
    string Url
);
