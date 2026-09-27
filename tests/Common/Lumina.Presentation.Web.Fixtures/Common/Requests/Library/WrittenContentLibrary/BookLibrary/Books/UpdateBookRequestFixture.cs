#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary;
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for generating <see cref="UpdateBookRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookRequestFixture
{
    private readonly Faker _faker = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly BookSeriesDtoFixture _bookSeriesDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="UpdateBookRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional Id of the book to update.</param>
    /// <param name="libraryId">Optional Id of the media library the book belongs to.</param>
    /// <param name="metadata">Optional written content metadata of the book.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFormat">Whether the format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeEdition">Whether the edition should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeVolumeNumber">Whether the volume number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSeries">Whether the series should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGoodreadsId">Whether the Goodreads Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLccn">Whether the LCCN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOclcNumber">Whether the OCLC number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOpenLibraryId">Whether the Open Library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryThingId">Whether the LibraryThing Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGoogleBooksId">Whether the Google Books Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarnesAndNobleId">Whether the Barnes and Noble Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAppleBooksId">Whether the Apple Books Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsbns">Whether the ISBNs should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="UpdateBookRequest"/> instance.</returns>
    public UpdateBookRequest Create(
        Guid? id = null,
        Guid? libraryId = null,
        WrittenContentMetadataDto? metadata = null,
        bool includeMetadata = true,
        bool includeFormat = true,
        bool includeEdition = true,
        bool includeVolumeNumber = true,
        bool includeSeries = true,
        bool includeAsin = true,
        bool includeGoodreadsId = true,
        bool includeLccn = true,
        bool includeOclcNumber = true,
        bool includeOpenLibraryId = true,
        bool includeLibraryThingId = true,
        bool includeGoogleBooksId = true,
        bool includeBarnesAndNobleId = true,
        bool includeAppleBooksId = true,
        bool includeIsbns = true,
        bool includeContributors = true,
        bool includeRatings = true)
    {
        return new UpdateBookRequest
        {
            BookId = id?.ToString() ?? Guid.NewGuid().ToString(),
            LibraryId = libraryId ?? Guid.NewGuid(),
            Metadata = includeMetadata ? (metadata ?? _writtenContentMetadataDtoFixture.Create()) : null,
            Format = includeFormat ? _faker.PickRandom<BookFormat>() : null,
            Edition = includeEdition ? _faker.Random.String2(_faker.Random.Number(1, 50)) : null,
            VolumeNumber = includeVolumeNumber ? _faker.Random.Number(1, 3) : null,
            Series = includeSeries ? _bookSeriesDtoFixture.Create() : null,
            ASIN = includeAsin ? _faker.Random.String2(10) : null,
            GoodreadsId = includeGoodreadsId ? _faker.Random.Number(100000, 500000).ToString() : null,
            LCCN = includeLccn ? "n78890351" : null,
            OCLCNumber = includeOclcNumber ? "ocm12345678" : null,
            OpenLibraryId = includeOpenLibraryId ? "OL123456M" : null,
            LibraryThingId = includeLibraryThingId ? _faker.Random.String2(_faker.Random.Number(1, 50)) : null,
            GoogleBooksId = includeGoogleBooksId ? CreateGoogleBooksId() : null,
            BarnesAndNobleId = includeBarnesAndNobleId ? _faker.Random.String2(10, "0123456789") : null,
            AppleBooksId = includeAppleBooksId ? $"id{_faker.Random.Number(1, 999999)}" : null,
            ISBNs = includeIsbns ? [.. _isbnDtoFixture.CreateMany(_faker.Random.Number(1, 3))] : null,
            Contributors = includeContributors ? [.. _mediaContributorDtoFixture.CreateMany(_faker.Random.Number(1, 3))] : null,
            Ratings = includeRatings ? [.. _bookRatingDtoFixture.CreateMany(_faker.Random.Number(1, 3))] : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateBookRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateBookRequest"/> instances.</returns>
    public List<UpdateBookRequest> CreateMany(int count = 3)
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
