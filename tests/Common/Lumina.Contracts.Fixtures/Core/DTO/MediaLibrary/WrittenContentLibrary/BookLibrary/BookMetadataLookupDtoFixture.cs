#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="BookMetadataLookupDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMetadataLookupDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BookMetadataLookupDto"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the book belongs to.</param>
    /// <param name="path">Optional. The file system path of the book.</param>
    /// <param name="isbn">Optional. The ISBN of the book.</param>
    /// <param name="openLibraryId">Optional. The Open Library ID of the book.</param>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="author">Optional. The author of the book.</param>
    /// <param name="languageCode">Optional. The language code of the book.</param>
    /// <returns>The created <see cref="BookMetadataLookupDto"/>.</returns>
    public BookMetadataLookupDto Create(
        Guid? libraryId = null,
        string? path = null,
        string? isbn = null,
        string? openLibraryId = null,
        string? title = null,
        string? author = null,
        string? languageCode = null)
    {
        return new BookMetadataLookupDto(
            libraryId ?? Guid.NewGuid(),
            path ?? _faker.System.FilePath(),
            isbn,
            openLibraryId,
            title,
            author,
            languageCode);
    }

    /// <summary>
    /// Creates a list of <see cref="BookMetadataLookupDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<BookMetadataLookupDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
