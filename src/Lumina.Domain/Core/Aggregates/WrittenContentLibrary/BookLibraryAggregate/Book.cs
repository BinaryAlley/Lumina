#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using ErrorOr;
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;

/// <summary>
/// Entity for a book.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class Book : Entity<BookId>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly List<MediaContributorId> _contributors;
    private readonly List<Rating> _ratings;
    private readonly List<Isbn> _isbns;
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the written content metadata of the book.
    /// </summary>
    public WrittenContentMetadata Metadata { get; private set; }

    /// <summary>
    /// Gets the format of the book (e.g., Hardcover, Paperback).
    /// </summary>
    public BookFormat Format { get; private set; }

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
    public Optional<string> Series { get; private set; }

    /// <summary>
    /// Gets the ASIN (Amazon Standard Identification Number) of the book.
    /// </summary>
    public Optional<string> ASIN { get; private set; }

    /// <summary>
    /// Gets the Goodreads ID of the book.
    /// </summary>
    public Optional<string> GoodreadsId { get; private set; }

    /// <summary>
    /// Gets the Library of Congress Control Number (LCCN) of the book.
    /// </summary>
    public Optional<string> LCCN { get; private set; }

    /// <summary>
    /// Gets the OCLC Number (WorldCat identifier) of the book.
    /// </summary>
    public Optional<string> OCLCNumber { get; private set; }

    /// <summary>
    /// Gets the Open Library ID of the book.
    /// </summary>
    public Optional<string> OpenLibraryId { get; private set; }

    /// <summary>
    /// Gets the LibraryThing ID of the book.
    /// </summary>
    public Optional<string> LibraryThingId { get; private set; }

    /// <summary>
    /// Gets the Google Books ID of the book.
    /// </summary>
    public Optional<string> GoogleBooksId { get; private set; }

    /// <summary>
    /// Gets the Barnes & Noble ID of the book.
    /// </summary>
    public Optional<string> BarnesAndNobleId { get; private set; }

    /// <summary>
    /// Gets the Kobo ID of the book.
    /// </summary>
    public Optional<string> KoboId { get; private set; }

    /// <summary>
    /// Gets the Apple Books ID of the book.
    /// </summary>
    public Optional<string> AppleBooksId { get; private set; }

    /// <summary>
    /// Gets the list of ISBN (International Standard Book Number) of the book.
    /// </summary>
    public IReadOnlyCollection<Isbn> ISBNs
    {
        get { return _isbns.AsReadOnly(); }
    }

    /// <summary>
    /// Gets the list of media contributors (actors, directors, etc) starring in this book.
    /// </summary>
    public IReadOnlyCollection<MediaContributorId> Contributors
    {
        get { return _contributors.AsReadOnly(); }
    }

    /// <summary>
    /// Gets the list of ratings for this book.
    /// </summary>
    public IReadOnlyCollection<Rating> Ratings
    {
        get { return _ratings.AsReadOnly(); }
    }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="Book"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the book.</param>
    /// <param name="metadata">The metadata of the book.</param>
    /// <param name="format">The format of the book (e.g., Hardcover, Paperback).</param>
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
    /// <param name="koboId">The optional Kobo ID of the book.</param>
    /// <param name="appleBooksId">The optional Apple Books ID of the book.</param>
    /// <param name="isbns">The list of ISBNs of the book.</param>
    /// <param name="contributors">The list of media contributors of the book.</param>
    /// <param name="ratings">The list of ratings for the book.</param>
    public Book(
        BookId id,
        WrittenContentMetadata metadata,
        BookFormat format,
        Optional<string> edition,
        Optional<int> volumeNumber,
        Optional<string> series,
        Optional<string> asin,
        Optional<string> goodreadsId,
        Optional<string> lccn,
        Optional<string> oclcNumber,
        Optional<string> openLibraryId,
        Optional<string> libraryThingId,
        Optional<string> googleBooksId,
        Optional<string> barnesAndNobleId,
        Optional<string> koboId,
        Optional<string> appleBooksId,
        List<Isbn> isbns,
        List<MediaContributorId> contributors,
        List<Rating> ratings) : base(id)
    {
        Id = id;
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
        KoboId = koboId;
        AppleBooksId = appleBooksId;
        _isbns = isbns;
        _contributors = contributors;
        _ratings = ratings;
    }

#pragma warning disable CS8618
    /// <summary>
    /// Initializes a new instance of the <see cref="Book"/> class.
    /// </summary>
    private Book() // only needed during reflection
    {

    }
#pragma warning restore CS8618
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="Book"/> class.
    /// </summary>
    /// <param name="metadata">The metadata of the book.</param>
    /// <param name="format">The format of the book (e.g., Hardcover, Paperback).</param>
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
    /// <param name="koboId">The optional Kobo ID of the book.</param>
    /// <param name="appleBooksId">The optional Apple Books ID of the book.</param>
    /// <param name="isbns">The list of ISBNs of the book.</param>
    /// <param name="contributors">The list of media contributors of the book.</param>
    /// <param name="ratings">The list of ratings for the book.</param>
    /// <returns>The created <see cref="Book"/>.</returns>
    public static ErrorOr<Book> Create(
        WrittenContentMetadata metadata,
        BookFormat format,
        Optional<string> edition,
        Optional<int> volumeNumber,
        Optional<string> series,
        Optional<string> asin,
        Optional<string> goodreadsId,
        Optional<string> lccn,
        Optional<string> oclcNumber,
        Optional<string> openLibraryId,
        Optional<string> libraryThingId,
        Optional<string> googleBooksId,
        Optional<string> barnesAndNobleId,
        Optional<string> koboId,
        Optional<string> appleBooksId,
        List<Isbn> isbns,
        List<MediaContributorId> contributors,
        List<Rating> ratings)
    {
        // TODO: enforce invariants
        return new Book(
            BookId.CreateUnique(),
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
            koboId,
            appleBooksId,
            isbns,
            contributors,
            ratings);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Book"/>, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the id of the book.</param>
    /// <param name="metadata">The metadata of the book.</param>
    /// <param name="format">The format of the book (e.g., Hardcover, Paperback).</param>
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
    /// <param name="koboId">The optional Kobo ID of the book.</param>
    /// <param name="appleBooksId">The optional Apple Books ID of the book.</param>
    /// <param name="isbns">The list of ISBNs of the book.</param>
    /// <param name="contributors">The list of media contributors of the book.</param>
    /// <param name="ratings">The list of ratings for the book.</param>
    /// <returns>The created <see cref="Book"/>.</returns>
    public static ErrorOr<Book> Create(
        BookId id,
        WrittenContentMetadata metadata,
        BookFormat format,
        Optional<string> edition,
        Optional<int> volumeNumber,
        Optional<string> series,
        Optional<string> asin,
        Optional<string> goodreadsId,
        Optional<string> lccn,
        Optional<string> oclcNumber,
        Optional<string> openLibraryId,
        Optional<string> libraryThingId,
        Optional<string> googleBooksId,
        Optional<string> barnesAndNobleId,
        Optional<string> koboId,
        Optional<string> appleBooksId,
        List<Isbn> isbns,
        List<MediaContributorId> contributors,
        List<Rating> ratings)
    {
        // TODO: enforce invariants
        return new Book(
            id,
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
            koboId,
            appleBooksId,
            isbns,
            contributors,
            ratings);
    }
    #endregion
}