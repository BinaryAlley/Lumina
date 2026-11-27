#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingDocumentDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingDocumentDtoFixture
{
    private readonly ReadingTocEntryDtoFixture _readingTocEntryDtoFixture = new();
    private readonly ReadingSpineItemDtoFixture _readingSpineItemDtoFixture = new();
    private readonly ReadingResourceInfoDtoFixture _readingResourceInfoDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ReadingDocumentDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="author">Optional. The author of the book, if known.</param>
    /// <param name="coverResourceKey">Optional. The resource key of the cover image of the book, if applicable.</param>
    /// <param name="tableOfContents">Optional. The hierarchical table of contents of the book.</param>
    /// <param name="spine">Optional. The ordered spine of the reading sections of the book.</param>
    /// <param name="resources">Optional. The resources of the book, keyed by their resource key.</param>
    /// <param name="hasTextContent">Optional. Whether the book has extractable text content.</param>
    /// <returns>The created <see cref="ReadingDocumentDto"/>.</returns>
    public ReadingDocumentDto Create(
        string? title = null,
        string? author = null,
        string? coverResourceKey = null,
        IReadOnlyList<ReadingTocEntryDto>? tableOfContents = null,
        IReadOnlyList<ReadingSpineItemDto>? spine = null,
        IReadOnlyDictionary<string, ReadingResourceInfoDto>? resources = null,
        bool? hasTextContent = null)
    {
        return new ReadingDocumentDto(
            title ?? $"Book {Guid.NewGuid():N}",
            author,
            coverResourceKey,
            tableOfContents ?? [.. _readingTocEntryDtoFixture.CreateMany(2)],
            spine ?? [.. _readingSpineItemDtoFixture.CreateMany(2)],
            resources ?? new Dictionary<string, ReadingResourceInfoDto>
            {
                [$"resource-{Guid.NewGuid():N}"] = _readingResourceInfoDtoFixture.Create()
            },
            hasTextContent ?? true
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingDocumentDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingDocumentDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
