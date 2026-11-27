#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents the reading manifest of a book, containing everything the client needs to render the reader.
/// </summary>
/// <param name="Title">The title of the book.</param>
/// <param name="Author">The author of the book, if known.</param>
/// <param name="CoverResourceKey">The resource key of the cover image of the book, if applicable.</param>
/// <param name="TableOfContents">The hierarchical table of contents of the book.</param>
/// <param name="Spine">The ordered spine of the reading sections of the book.</param>
/// <param name="ResourceKeys">The resource keys of the resources of the book.</param>
/// <param name="HasTextContent">Whether the book has extractable text content. A scanned book, whose pages are only images, has no text content.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record ReadingManifestResponse(
    string Title,
    string? Author,
    string? CoverResourceKey,
    IReadOnlyList<ReadingTocEntryResponse> TableOfContents,
    IReadOnlyList<ReadingSpineItemResponse> Spine,
    IReadOnlyList<string> ResourceKeys,
    bool HasTextContent
);
