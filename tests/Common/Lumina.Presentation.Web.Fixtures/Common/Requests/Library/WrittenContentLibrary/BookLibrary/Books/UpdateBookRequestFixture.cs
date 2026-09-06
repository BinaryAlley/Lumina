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
    /// <param name="metadata">Optional written content metadata of the book.</param>
    /// <param name="includeOptionalProperties">Whether the properties that are not explicitly provided should be randomized, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="UpdateBookRequest"/> instance.</returns>
    public UpdateBookRequest Create(
        Guid? id = null,
        WrittenContentMetadataDto? metadata = null,
        bool includeOptionalProperties = true)
    {
        return new UpdateBookRequest
        {
            Id = id?.ToString() ?? Guid.NewGuid().ToString(),
            Metadata = metadata ?? (includeOptionalProperties ? _writtenContentMetadataDtoFixture.Create() : null),
            Format = includeOptionalProperties ? _faker.PickRandom<BookFormat>() : null,
            Edition = includeOptionalProperties ? _faker.Random.String2(_faker.Random.Number(1, 50)) : null,
            VolumeNumber = includeOptionalProperties ? _faker.Random.Number(1, 3) : null,
            Series = includeOptionalProperties ? _bookSeriesDtoFixture.Create() : null,
            ASIN = includeOptionalProperties ? _faker.Random.String2(10) : null,
            GoodreadsId = includeOptionalProperties ? _faker.Random.Number(100000, 500000).ToString() : null,
            LCCN = includeOptionalProperties ? "n78890351" : null,
            OCLCNumber = includeOptionalProperties ? "ocm12345678" : null,
            OpenLibraryId = includeOptionalProperties ? "OL123456M" : null,
            LibraryThingId = includeOptionalProperties ? _faker.Random.String2(_faker.Random.Number(1, 50)) : null,
            GoogleBooksId = includeOptionalProperties ? CreateGoogleBooksId() : null,
            BarnesAndNobleId = includeOptionalProperties ? _faker.Random.String2(10, "0123456789") : null,
            AppleBooksId = includeOptionalProperties ? $"id{_faker.Random.Number(1, 999999)}" : null,
            ISBNs = includeOptionalProperties ? [.. _isbnDtoFixture.CreateMany(_faker.Random.Number(1, 3))] : null,
            Contributors = includeOptionalProperties ? [.. _mediaContributorDtoFixture.CreateMany(_faker.Random.Number(1, 3))] : null,
            Ratings = includeOptionalProperties ? [.. _bookRatingDtoFixture.CreateMany(_faker.Random.Number(1, 3))] : null
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
