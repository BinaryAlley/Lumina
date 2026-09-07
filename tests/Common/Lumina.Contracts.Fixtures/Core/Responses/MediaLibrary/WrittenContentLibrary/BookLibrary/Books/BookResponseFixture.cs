#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for the <see cref="BookResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookResponseFixture
{
    private readonly Faker _faker = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly BookSeriesDtoFixture _bookSeriesDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="BookResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the book.</param>
    /// <param name="libraryId">Optional. The Id of the media library the book belongs to.</param>
    /// <param name="path">Optional. The file system path of the book.</param>
    /// <param name="metadata">Optional. The written content metadata of the book.</param>
    /// <param name="format">Optional. The format of the book.</param>
    /// <param name="metadataStatus">Optional. The metadata enrichment status of the book.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the book was created.</param>
    /// <param name="coverPath">Optional. The file system path of the cover image of the book.</param>
    /// <returns>The created <see cref="BookResponse"/>.</returns>
    public BookResponse Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? path = null,
        WrittenContentMetadataDto? metadata = null,
        BookFormat? format = null,
        MetadataStatus? metadataStatus = null,
        DateTime? createdOnUtc = null,
        string? coverPath = null)
    {
        DateTime resolvedCreatedOnUtc = createdOnUtc ?? _faker.Date.Past().ToUniversalTime();
        return new BookResponse(
            id ?? Guid.NewGuid(),
            libraryId ?? Guid.NewGuid(),
            path ?? _faker.System.FilePath(),
            metadata ?? _writtenContentMetadataDtoFixture.Create(),
            format ?? _faker.PickRandom<BookFormat>(),
            _faker.Lorem.Word(),
            _faker.Random.Int(1, 10),
            _bookSeriesDtoFixture.Create(),
            _faker.Random.AlphaNumeric(10),
            _faker.Random.AlphaNumeric(5),
            _faker.Random.AlphaNumeric(8),
            _faker.Random.AlphaNumeric(10),
            _faker.Random.AlphaNumeric(7),
            _faker.Random.AlphaNumeric(7),
            _faker.Random.AlphaNumeric(6),
            _faker.Random.AlphaNumeric(10),
            _faker.Random.AlphaNumeric(8),
            _isbnDtoFixture.CreateMany(1),
            _mediaContributorDtoFixture.CreateMany(1),
            _bookRatingDtoFixture.CreateMany(1),
            metadataStatus ?? _faker.PickRandom<MetadataStatus>(),
            _faker.Date.Recent().ToUniversalTime(),
            _faker.Company.CompanyName(),
            resolvedCreatedOnUtc,
            _faker.Random.Bool() ? _faker.Date.Recent().ToUniversalTime() : null,
            coverPath ?? (_faker.Random.Bool() ? _faker.System.FilePath() : null)
        );
    }

    /// <summary>
    /// Creates a list of <see cref="BookResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<BookResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
