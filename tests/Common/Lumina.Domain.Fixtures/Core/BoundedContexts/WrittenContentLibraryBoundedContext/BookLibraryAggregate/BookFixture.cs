#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;

/// <summary>
/// Fixture class for the <see cref="Book"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookFixture
{
    private readonly Faker _faker = new();
    private readonly WrittenContentMetadataFixture _writtenContentMetadataFixture = new();
    private readonly IsbnFixture _isbnFixture = new();
    private readonly BookRatingFixture _bookRatingFixture = new();
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="Book"/> domain aggregate.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library this book belongs to.</param>
    /// <param name="path">Optional. The file system path of the book.</param>
    /// <param name="metadata">Optional. The written content metadata of the book.</param>
    /// <param name="format">Optional. The format of the book.</param>
    /// <param name="edition">Optional. The edition of the book.</param>
    /// <param name="volumeNumber">Optional. The volume or book number in the series.</param>
    /// <param name="series">Optional. The series the book is part of.</param>
    /// <param name="asin">Optional. The ASIN of the book.</param>
    /// <param name="goodreadsId">Optional. The Goodreads Id of the book.</param>
    /// <param name="lccn">Optional. The LCCN of the book.</param>
    /// <param name="oclcNumber">Optional. The OCLC number of the book.</param>
    /// <param name="openLibraryId">Optional. The Open Library Id of the book.</param>
    /// <param name="libraryThingId">Optional. The LibraryThing Id of the book.</param>
    /// <param name="googleBooksId">Optional. The Google Books Id of the book.</param>
    /// <param name="barnesAndNobleId">Optional. The Barnes and Noble Id of the book.</param>
    /// <param name="appleBooksId">Optional. The Apple Books Id of the book.</param>
    /// <param name="isbns">Optional. The ISBNs of the book.</param>
    /// <param name="contributors">Optional. The media contributors of the book.</param>
    /// <param name="ratings">Optional. The ratings of the book.</param>
    /// <returns>The created <see cref="Book"/> domain aggregate.</returns>
    public Book Create(
        Guid? libraryId = null,
        string? path = null,
        WrittenContentMetadata? metadata = null,
        BookFormat? format = null,
        string? edition = null,
        float? volumeNumber = null,
        BookSeries? series = null,
        string? asin = null,
        string? goodreadsId = null,
        string? lccn = null,
        string? oclcNumber = null,
        string? openLibraryId = null,
        string? libraryThingId = null,
        string? googleBooksId = null,
        string? barnesAndNobleId = null,
        string? appleBooksId = null,
        List<Isbn>? isbns = null,
        List<MediaContributorId>? contributors = null,
        List<BookRating>? ratings = null)
    {
        return Book.Create(
            libraryId is null ? LibraryId.Create(Guid.NewGuid()) : LibraryId.Create(libraryId.Value),
            path ?? _faker.System.FilePath(),
            metadata ?? _writtenContentMetadataFixture.Create(),
            format ?? _faker.PickRandom<BookFormat>(),
            edition is null ? Optional<string>.Some(_faker.Commerce.ProductName()) : Optional<string>.Some(edition),
            volumeNumber is null ? Optional<float>.None() : Optional<float>.Some(volumeNumber.Value),
            series is null ? Optional<BookSeries>.None() : Optional<BookSeries>.Some(series),
            asin is null ? Optional<string>.Some(_faker.Random.String2(10)) : Optional<string>.Some(asin),
            goodreadsId is null ? Optional<string>.Some(_faker.Random.Number(100000, 500000).ToString()) : Optional<string>.Some(goodreadsId),
            lccn is null ? Optional<string>.Some(_faker.Random.String2(_faker.Random.Number(8, 10))) : Optional<string>.Some(lccn),
            oclcNumber is null ? Optional<string>.Some($"ocm{_faker.Random.String2(8, "0123456789")}") : Optional<string>.Some(oclcNumber),
            openLibraryId is null ? Optional<string>.Some($"OL{_faker.Random.Number(100000, 999999)}{_faker.Random.ArrayElement(['A', 'M', 'W'])}") : Optional<string>.Some(openLibraryId),
            libraryThingId is null ? Optional<string>.Some(_faker.Random.String2(_faker.Random.Number(1, 50))) : Optional<string>.Some(libraryThingId),
            googleBooksId is null ? Optional<string>.Some(_faker.Random.String2(12)) : Optional<string>.Some(googleBooksId),
            barnesAndNobleId is null ? Optional<string>.Some(_faker.Random.String2(10, "0123456789")) : Optional<string>.Some(barnesAndNobleId),
            appleBooksId is null ? Optional<string>.Some($"id{_faker.Random.Number(1, 999999)}") : Optional<string>.Some(appleBooksId),
            isbns ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 4)).Select(_ => _isbnFixture.Create())],
            contributors ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 4)).Select(_ => _mediaContributorIdFixture.Create())],
            ratings ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 4)).Select(_ => _bookRatingFixture.Create())]
        ).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Book"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Book"/> instances.</returns>
    public List<Book> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
