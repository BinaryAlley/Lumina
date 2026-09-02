#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Data transfer object for the normalized document model of a book, produced by a reader plugin.
/// </summary>
/// <param name="Title">The title of the book.</param>
/// <param name="Author">The author of the book, if known.</param>
/// <param name="CoverResourceKey">The resource key of the cover image of the book, if applicable.</param>
/// <param name="TableOfContents">The hierarchical table of contents of the book.</param>
/// <param name="Spine">The ordered spine of the reading sections of the book.</param>
/// <param name="Resources">The resources of the book, keyed by their resource key.</param>
/// <param name="HasTextContent">Whether the book has extractable text content. A scanned book, whose pages are only images, has no text content.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record ReadingDocumentDto(
    string Title,
    string? Author,
    string? CoverResourceKey,
    IReadOnlyList<ReadingTocEntryDto> TableOfContents,
    IReadOnlyList<ReadingSpineItemDto> Spine,
    IReadOnlyDictionary<string, ReadingResourceInfoDto> Resources,
    bool HasTextContent
);
