#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="BookMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMetadataDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BookMetadataDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="originalTitle">Optional. The original title of the book.</param>
    /// <param name="description">Optional. A brief description or summary of the book.</param>
    /// <param name="goodreadsId">Optional. The Goodreads ID of the book.</param>
    /// <param name="format">Optional. The format of the book.</param>
    /// <param name="publisher">Optional. The name of the publisher of the book.</param>
    /// <param name="pageCount">Optional. The number of pages in the book.</param>
    /// <param name="includeOptionalProperties">Whether optional properties should be included.</param>
    /// <returns>The created <see cref="BookMetadataDto"/>.</returns>
    public BookMetadataDto Create(
        string? title = null,
        string? originalTitle = null,
        string? description = null,
        string? goodreadsId = null,
        BookFormat? format = null,
        string? publisher = null,
        int? pageCount = null,
        bool includeOptionalProperties = true)
    {
        int releaseYear = _faker.Random.Int(1900, 2024);
        return new BookMetadataDto(
            title ?? _faker.Lorem.Sentence(3),
            originalTitle,
            description,
            includeOptionalProperties
                ? new ReleaseInfoDto(new DateOnly(releaseYear, 1, 1), releaseYear, null, null, _faker.Address.CountryCode(), null)
                : null,
            includeOptionalProperties ? [new GenreDto(_faker.Lorem.Word())] : null,
            includeOptionalProperties ? [new TagDto(_faker.Lorem.Word())] : null,
            includeOptionalProperties ? new LanguageInfoDto("en", "English", "English") : null,
            null,
            publisher,
            pageCount,
            format ?? _faker.PickRandom<BookFormat>(),
            null,
            includeOptionalProperties ? _faker.Random.Int(1, 10) : null,
            null,
            null,
            goodreadsId,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    /// <summary>
    /// Creates a list of <see cref="BookMetadataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<BookMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
