#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Reading;

/// <summary>
/// Data transfer object for the reading manifest of a book.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class ReadingManifestDto
{
    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the book, if known.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the resource key of the cover image of the book, if applicable.
    /// </summary>
    public string? CoverResourceKey { get; set; }

    /// <summary>
    /// Gets or sets the hierarchical table of contents of the book.
    /// </summary>
    public List<ReadingTocEntryDto> TableOfContents { get; set; } = [];

    /// <summary>
    /// Gets or sets the ordered spine of the reading sections of the book.
    /// </summary>
    public List<ReadingSpineItemDto> Spine { get; set; } = [];

    /// <summary>
    /// Gets or sets the resource keys of the resources of the book.
    /// </summary>
    public List<string> ResourceKeys { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the book has extractable text content. A scanned book, whose pages are only images, has no text content.
    /// </summary>
    public bool HasTextContent { get; set; }
}
