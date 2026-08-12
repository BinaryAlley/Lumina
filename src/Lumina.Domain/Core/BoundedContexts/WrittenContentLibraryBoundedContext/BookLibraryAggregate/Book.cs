#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;

/// <summary>
/// Aggregate root for a book.
/// </summary>
[DebuggerDisplay("Id: {Id} Title: {Title}")]
public sealed class Book : AggregateRoot<BookId>
{
    private readonly List<MediaContributorId> _contributors;
    private readonly List<BookRating> _ratings;
    private readonly List<Isbn> _isbns;

    /// <summary>
    /// Gets the written content metadata of the book.
    /// </summary>
    public WrittenContentMetadata Metadata { get; private set; }

    /// <summary>
    /// Gets the Id of the media library this book belongs to.
    /// </summary>
    public LibraryId LibraryId { get; private set; }

    /// <summary>
    /// Gets the file system path of the book.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Gets the status of the metadata enrichment of the book.
    /// </summary>
    public MetadataStatus MetadataStatus { get; private set; }

    /// <summary>
    /// Gets the date and time when the metadata of the book was last enriched, if applicable.
    /// </summary>
    public Optional<DateTime> LastMetadataUpdateUtc { get; private set; }

    /// <summary>
    /// Gets the name of the plugin that enriched the metadata of the book, if applicable.
    /// </summary>
    public Optional<string> MetadataProvider { get; private set; }

    /// <summary>
    /// Gets the format of the book (e.g., Hardcover, Paperback), if applicable.
    /// </summary>
    public Optional<BookFormat> Format { get; private set; }

    /// <summary>
    /// Gets the edition of the book, if applicable.
    /// </summary>
    public Optional<string> Edition { get; private set; }

    /// <summary>
    /// Gets the volume or book number in the series, if applicable.
    /// </summary>
    public Optional<int> VolumeNumber { get; private set; }

    /// <summary>
    /// Gets the series name, if the book is part of a series.
    /// </summary>
    public Optional<BookSeries> Series { get; private set; }

    /// <summary>
    /// Gets the ASIN (Amazon Standard Identification Number) of the book, if applicable.
    /// </summary>
    public Optional<string> ASIN { get; private set; }

    /// <summary>
    /// Gets the Goodreads ID of the book, if applicable.
    /// </summary>
    public Optional<string> GoodreadsId { get; private set; }

    /// <summary>
    /// Gets the Library of Congress Control Number (LCCN) of the book, if applicable.
    /// </summary>
    public Optional<string> LCCN { get; private set; }

    /// <summary>
    /// Gets the OCLC Number (WorldCat identifier) of the book, if applicable.
    /// </summary>
    public Optional<string> OCLCNumber { get; private set; }

    /// <summary>
    /// Gets the Open Library ID of the book, if applicable.
    /// </summary>
    public Optional<string> OpenLibraryId { get; private set; }

    /// <summary>
    /// Gets the LibraryThing ID of the book, if applicable.
    /// </summary>
    public Optional<string> LibraryThingId { get; private set; }

    /// <summary>
    /// Gets the Google Books ID of the book, if applicable.
    /// </summary>
    public Optional<string> GoogleBooksId { get; private set; }

    /// <summary>
    /// Gets the Barnes & Noble ID of the book, if applicable.
    /// </summary>
    public Optional<string> BarnesAndNobleId { get; private set; }

    /// <summary>
    /// Gets the Apple Books ID of the book, if applicable.
    /// </summary>
    public Optional<string> AppleBooksId { get; private set; }

    /// <summary>
    /// Gets the list of ISBN (International Standard Book Number) of the book.
    /// </summary>
    public IReadOnlyCollection<Isbn> ISBNs => _isbns.AsReadOnly();

    /// <summary>
    /// Gets the list of objects representing the unique identifiers of the media contributors (actors, directors, etc) starring in this book.
    /// </summary>
    public IReadOnlyCollection<MediaContributorId> Contributors => _contributors.AsReadOnly();

    /// <summary>
    /// Gets the list of ratings for this book.
    /// </summary>
    public IReadOnlyCollection<BookRating> Ratings => _ratings.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Book"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the book.</param>
    /// <param name="metadata">The metadata of the book.</param>
    /// <param name="format">The optional format of the book (e.g., Hardcover, Paperback).</param>
    /// <param name="edition">The optional edition of the book.</param>
    /// <param name="volumeNumber">The optional volume or book number in the series.</param>
    /// <param name="series">The optional series name, if the book is part of a series.</param>
    /// <param name="asin">The optional ASIN of the book.</param>
    /// <param name="goodreadsId">The optional Goodreads ID of the book.</param>
    /// <param name="lccn">The optional LCCN of the book.</param>
    /// <param name="oclcNumber">The optional OCLC Number of the book.</param>
    /// <param name="openLibraryId">The optional Open Library ID of the book.</param>
    /// <param name="libraryThingId">The optional LibraryThing ID of the book.</param>
    /// <param name="googleBooksId">The optional Google Books ID of the book.</param>
    /// <param name="barnesAndNobleId">The optional Barnes & Noble ID of the book.</param>
    /// <param name="appleBooksId">The optional Apple Books ID of the book.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    /// <param name="isbns">The list of ISBNs of the book.</param>
    /// <param name="contributors">The list of objects representing the unique identifiers of the media contributors of the book.</param>
    /// <param name="ratings">The list of ratings for the book.</param>
    private Book(
        BookId id,
        LibraryId libraryId,
        string path,
        WrittenContentMetadata metadata,
        Optional<BookFormat> format,
        Optional<string> edition,
        Optional<int> volumeNumber,
        Optional<BookSeries> series,
        Optional<string> asin,
        Optional<string> goodreadsId,
        Optional<string> lccn,
        Optional<string> oclcNumber,
        Optional<string> openLibraryId,
        Optional<string> libraryThingId,
        Optional<string> googleBooksId,
        Optional<string> barnesAndNobleId,
        Optional<string> appleBooksId,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc,
        List<Isbn> isbns,
        List<MediaContributorId> contributors,
        List<BookRating> ratings) : base(id)
    {
        Id = id;
        LibraryId = libraryId;
        Path = path;
        Metadata = metadata;
        MetadataStatus = MetadataStatus.Pending;
        LastMetadataUpdateUtc = Optional<DateTime>.None();
        MetadataProvider = Optional<string>.None();
        Format = format;
        Edition = edition;
        VolumeNumber = volumeNumber;
        Series = series;
        ASIN = asin;
        GoodreadsId = goodreadsId;
        LCCN = lccn;
        OCLCNumber = oclcNumber;
        OpenLibraryId = openLibraryId;
        LibraryThingId = libraryThingId;
        GoogleBooksId = googleBooksId;
        BarnesAndNobleId = barnesAndNobleId;
        AppleBooksId = appleBooksId;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc.HasValue ? updatedOnUtc.Value : null;
        _isbns = isbns;
        _contributors = contributors;
        _ratings = ratings;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Book"/> class.
    /// </summary>
    /// <param name="libraryId">The Id of the media library this book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="metadata">The metadata of the book.</param>
    /// <param name="format">The optional format of the book (e.g., Hardcover, Paperback).</param>
    /// <param name="edition">The optional edition of the book.</param>
    /// <param name="volumeNumber">The optional volume or book number in the series.</param>
    /// <param name="series">The optional series name, if the book is part of a series.</param>
    /// <param name="asin">The optional ASIN of the book.</param>
    /// <param name="goodreadsId">The optional Goodreads ID of the book.</param>
    /// <param name="lccn">The optional LCCN of the book.</param>
    /// <param name="oclcNumber">The optional OCLC Number of the book.</param>
    /// <param name="openLibraryId">The optional Open Library ID of the book.</param>
    /// <param name="libraryThingId">The optional LibraryThing ID of the book.</param>
    /// <param name="googleBooksId">The optional Google Books ID of the book.</param>
    /// <param name="barnesAndNobleId">The optional Barnes & Noble ID of the book.</param>
    /// <param name="appleBooksId">The optional Apple Books ID of the book.</param>
    /// <param name="isbns">The list of ISBNs of the book.</param>
    /// <param name="contributors">The list of objects representing the unique identifiers of the media contributors of the book.</param>
    /// <param name="ratings">The list of ratings for the book.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Book"/>, or an error message.
    /// </returns>
    public static ErrorOr<Book> Create(
        LibraryId libraryId,
        string path,
        WrittenContentMetadata metadata,
        Optional<BookFormat> format,
        Optional<string> edition,
        Optional<int> volumeNumber,
        Optional<BookSeries> series,
        Optional<string> asin,
        Optional<string> goodreadsId,
        Optional<string> lccn,
        Optional<string> oclcNumber,
        Optional<string> openLibraryId,
        Optional<string> libraryThingId,
        Optional<string> googleBooksId,
        Optional<string> barnesAndNobleId,
        Optional<string> appleBooksId,
        List<Isbn> isbns,
        List<MediaContributorId> contributors,
        List<BookRating> ratings)
    {
        // TODO: enforce invariants
        return new Book(
            BookId.CreateUnique(),
            libraryId,
            path,
            metadata,
            format,
            edition,
            volumeNumber,
            series,
            asin,
            goodreadsId,
            lccn,
            oclcNumber,
            openLibraryId,
            libraryThingId,
            googleBooksId,
            barnesAndNobleId,
            appleBooksId,
            DateTime.UtcNow, // TODO: should be IDateTimeProvider
            default,
            isbns,
            contributors,
            ratings
        );
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Book"/>, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the book.</param>
    /// <param name="libraryId">The Id of the media library this book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="metadata">The metadata of the book.</param>
    /// <param name="format">The optional format of the book (e.g., Hardcover, Paperback).</param>
    /// <param name="edition">The optional edition of the book.</param>
    /// <param name="volumeNumber">The optional volume or book number in the series.</param>
    /// <param name="series">The optional series name, if the book is part of a series.</param>
    /// <param name="asin">The optional ASIN of the book.</param>
    /// <param name="goodreadsId">The optional Goodreads ID of the book.</param>
    /// <param name="lccn">The optional LCCN of the book.</param>
    /// <param name="oclcNumber">The optional OCLC Number of the book.</param>
    /// <param name="openLibraryId">The optional Open Library ID of the book.</param>
    /// <param name="libraryThingId">The optional LibraryThing ID of the book.</param>
    /// <param name="googleBooksId">The optional Google Books ID of the book.</param>
    /// <param name="barnesAndNobleId">The optional Barnes & Noble ID of the book.</param>
    /// <param name="appleBooksId">The optional Apple Books ID of the book.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    /// <param name="isbns">The list of ISBNs of the book.</param>
    /// <param name="contributors">The list of objects representing the unique identifiers of the media contributors of the book.</param>
    /// <param name="ratings">The list of ratings for the book.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Book"/>, or an error message.
    /// </returns>
    public static ErrorOr<Book> Create(
        BookId id,
        LibraryId libraryId,
        string path,
        WrittenContentMetadata metadata,
        Optional<BookFormat> format,
        Optional<string> edition,
        Optional<int> volumeNumber,
        Optional<BookSeries> series,
        Optional<string> asin,
        Optional<string> goodreadsId,
        Optional<string> lccn,
        Optional<string> oclcNumber,
        Optional<string> openLibraryId,
        Optional<string> libraryThingId,
        Optional<string> googleBooksId,
        Optional<string> barnesAndNobleId,
        Optional<string> appleBooksId,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc,
        List<Isbn> isbns,
        List<MediaContributorId> contributors,
        List<BookRating> ratings)
    {
        // TODO: enforce invariants
        return new Book(
            id,
            libraryId,
            path,
            metadata,
            format,
            edition,
            volumeNumber,
            series,
            asin,
            goodreadsId,
            lccn,
            oclcNumber,
            openLibraryId,
            libraryThingId,
            googleBooksId,
            barnesAndNobleId,
            appleBooksId,
            createdOnUtc,
            updatedOnUtc,
            isbns,
            contributors,
            ratings
        );
    }

    /// <summary>
    /// Marks the metadata of the book as enriched by the provided <paramref name="providerName"/>.
    /// </summary>
    /// <param name="providerName">The name of the metadata provider that enriched the book.</param>
    /// <param name="lastUpdateUtc">The date and time when the metadata of the book was enriched.</param>
    public void MarkMetadataAsEnriched(string providerName, DateTime lastUpdateUtc)
    {
        MetadataProvider = providerName;
        LastMetadataUpdateUtc = lastUpdateUtc;
        MetadataStatus = MetadataStatus.Enriched;
    }

    /// <summary>
    /// Marks the metadata enrichment of the book as failed.
    /// </summary>
    public void MarkMetadataAsFailed()
    {
        MetadataStatus = MetadataStatus.Failed;
    }

    /// <summary>
    /// Applies the enriched <paramref name="metadata"/> and the related fields to the book, marking its metadata as enriched by the provided <paramref name="providerName"/>.
    /// </summary>
    /// <param name="metadata">The enriched metadata of the book.</param>
    /// <param name="format">The optional format of the book.</param>
    /// <param name="edition">The optional edition of the book.</param>
    /// <param name="volumeNumber">The optional volume or book number in the series.</param>
    /// <param name="series">The optional series the book is part of.</param>
    /// <param name="asin">The optional ASIN of the book.</param>
    /// <param name="goodreadsId">The optional Goodreads ID of the book.</param>
    /// <param name="lccn">The optional LCCN of the book.</param>
    /// <param name="oclcNumber">The optional OCLC Number of the book.</param>
    /// <param name="openLibraryId">The optional Open Library ID of the book.</param>
    /// <param name="libraryThingId">The optional LibraryThing ID of the book.</param>
    /// <param name="googleBooksId">The optional Google Books ID of the book.</param>
    /// <param name="barnesAndNobleId">The optional Barnes &amp; Noble ID of the book.</param>
    /// <param name="appleBooksId">The optional Apple Books ID of the book.</param>
    /// <param name="isbns">The list of ISBNs of the book.</param>
    /// <param name="ratings">The list of ratings for the book.</param>
    /// <param name="providerName">The name of the metadata provider that enriched the book.</param>
    /// <param name="lastUpdateUtc">The date and time when the metadata was enriched.</param>
    public void ApplyEnrichedMetadata(
        WrittenContentMetadata metadata,
        Optional<BookFormat> format,
        Optional<string> edition,
        Optional<int> volumeNumber,
        Optional<BookSeries> series,
        Optional<string> asin,
        Optional<string> goodreadsId,
        Optional<string> lccn,
        Optional<string> oclcNumber,
        Optional<string> openLibraryId,
        Optional<string> libraryThingId,
        Optional<string> googleBooksId,
        Optional<string> barnesAndNobleId,
        Optional<string> appleBooksId,
        List<Isbn> isbns,
        List<BookRating> ratings,
        string providerName,
        DateTime lastUpdateUtc)
    {
        Metadata = metadata;
        Format = format;
        Edition = edition;
        VolumeNumber = volumeNumber;
        Series = series;
        ASIN = asin;
        GoodreadsId = goodreadsId;
        LCCN = lccn;
        OCLCNumber = oclcNumber;
        OpenLibraryId = openLibraryId;
        LibraryThingId = libraryThingId;
        GoogleBooksId = googleBooksId;
        BarnesAndNobleId = barnesAndNobleId;
        AppleBooksId = appleBooksId;
        // replace the contents of the collections in place, preserving the readonly reference invariants of the aggregate
        _isbns.Clear();
        _isbns.AddRange(isbns);
        _ratings.Clear();
        _ratings.AddRange(ratings);
        MarkMetadataAsEnriched(providerName, lastUpdateUtc);
    }
}
