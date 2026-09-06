#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for generating <see cref="BookDetailsDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookDetailsDtoFixture
{
    private readonly Faker _faker = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="BookDetailsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional Id of the book.</param>
    /// <param name="libraryId">Optional Id of the media library the book belongs to.</param>
    /// <param name="path">Optional file system path of the book.</param>
    /// <param name="metadata">Optional written content metadata of the book.</param>
    /// <param name="includeMetadata">Whether the written content metadata should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="BookDetailsDto"/> instance.</returns>
    public BookDetailsDto Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? path = null,
        WrittenContentMetadataDto? metadata = null,
        bool includeMetadata = true)
    {
        return new BookDetailsDto
        {
            Id = id ?? _faker.Random.Guid(),
            LibraryId = libraryId ?? _faker.Random.Guid(),
            Path = path ?? _faker.System.FilePath(),
            Metadata = metadata ?? (includeMetadata ? _writtenContentMetadataDtoFixture.Create() : null),
            Format = _faker.PickRandom<BookFormat>(),
            Edition = _faker.Random.String2(_faker.Random.Number(1, 50)),
            VolumeNumber = _faker.Random.Number(1, 3),
            ASIN = _faker.Random.String2(10),
            GoodreadsId = _faker.Random.Number(100000, 500000).ToString(),
            LCCN = "n78890351",
            OCLCNumber = "ocm12345678",
            OpenLibraryId = "OL123456M",
            LibraryThingId = _faker.Random.String2(_faker.Random.Number(1, 50)),
            GoogleBooksId = CreateGoogleBooksId(),
            BarnesAndNobleId = _faker.Random.String2(10, "0123456789"),
            AppleBooksId = $"id{_faker.Random.Number(1, 999999)}",
            ISBNs = [.. _isbnDtoFixture.CreateMany(_faker.Random.Number(1, 3))],
            Contributors = [.. _mediaContributorDtoFixture.CreateMany(_faker.Random.Number(1, 3))],
            Ratings = [.. _bookRatingDtoFixture.CreateMany(_faker.Random.Number(1, 3))],
            MetadataStatus = _faker.Random.String2(_faker.Random.Number(1, 20)),
            LastMetadataUpdateUtc = _faker.Date.Recent(),
            MetadataProvider = _faker.Random.String2(_faker.Random.Number(1, 20)),
            CreatedOnUtc = _faker.Date.Past(),
            UpdatedOnUtc = _faker.Date.Recent(),
            CoverPath = _faker.System.FilePath()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="BookDetailsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookDetailsDto"/> instances.</returns>
    public List<BookDetailsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Generates a valid Google Books Id.
    /// </summary>
    /// <returns>The generated Google Books Id.</returns>
    private string CreateGoogleBooksId()
    {
        const string VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        return new string([.. Enumerable.Repeat(VALID_CHARS, 12).Select(s => s[_faker.Random.Number(VALID_CHARS.Length - 1)])]);
    }
}
