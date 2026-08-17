#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Libraries;

/// <summary>
/// Data transfer object for a lightweight book element, containing only the properties needed by the client for card-style navigation.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class BookLiteDto
{
    /// <summary>
    /// Gets or sets the Id of the book.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the release year of the book (re-release year, if available, or original release year), if known.
    /// </summary>
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the path of the image representing the cover of the book, if available.
    /// </summary>
    public string? CoverPath { get; set; }
}
