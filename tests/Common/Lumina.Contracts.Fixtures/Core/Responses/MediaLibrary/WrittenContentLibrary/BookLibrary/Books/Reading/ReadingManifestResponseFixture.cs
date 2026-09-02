#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingManifestResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingManifestResponseFixture
{
    private readonly ReadingTocEntryResponseFixture _readingTocEntryResponseFixture = new();
    private readonly ReadingSpineItemResponseFixture _readingSpineItemResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ReadingManifestResponse"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="author">Optional. The author of the book, if known.</param>
    /// <param name="coverResourceKey">Optional. The resource key of the cover image of the book, if applicable.</param>
    /// <param name="tableOfContents">Optional. The hierarchical table of contents of the book.</param>
    /// <param name="spine">Optional. The ordered spine of the reading sections of the book.</param>
    /// <param name="resourceKeys">Optional. The resource keys of the resources of the book.</param>
    /// <param name="hasTextContent">Optional. Whether the book has extractable text content.</param>
    /// <returns>The created <see cref="ReadingManifestResponse"/>.</returns>
    public ReadingManifestResponse Create(
        string? title = null,
        string? author = null,
        string? coverResourceKey = null,
        IReadOnlyList<ReadingTocEntryResponse>? tableOfContents = null,
        IReadOnlyList<ReadingSpineItemResponse>? spine = null,
        IReadOnlyList<string>? resourceKeys = null,
        bool? hasTextContent = null)
    {
        return new ReadingManifestResponse(
            title ?? $"Book {Guid.NewGuid():N}",
            author,
            coverResourceKey,
            tableOfContents ?? [.. _readingTocEntryResponseFixture.CreateMany(2)],
            spine ?? [.. _readingSpineItemResponseFixture.CreateMany(2)],
            resourceKeys ?? [.. Enumerable.Range(0, 2).Select(_ => $"resource-{Guid.NewGuid():N}")],
            hasTextContent ?? true
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingManifestResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingManifestResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
