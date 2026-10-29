#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.BookLibrary;

/// <summary>
/// Contains integration tests for the <see cref="BookRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRepositoryTests
{
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();
    private readonly BookRatingEntityFixture _bookRatingEntityFixture = new();
    private readonly IsbnEntityFixture _isbnEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();

    [Fact]
    public async Task ResetEnrichmentStateForPathsAsync_WhenCalled_ShouldResetTheMetadataAndArtworkStatusesForThePaths()
    {
        // Arrange
        // The reset methods use ExecuteUpdateAsync, which is not supported by the in-memory provider, so a real SQLite database is used.
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-reset-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity changedBook = _bookEntityFixture.Create();
        changedBook.LibraryId = libraryId;
        changedBook.Path = "/books/changed.epub";
        changedBook.MetadataStatus = MetadataStatus.Enriched;
        changedBook.Artwork = [_bookArtworkEntityFixture.Create(bookId: changedBook.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity unchangedBook = _bookEntityFixture.Create();
        unchangedBook.LibraryId = libraryId;
        unchangedBook.Path = "/books/unchanged.epub";
        unchangedBook.MetadataStatus = MetadataStatus.Enriched;
        unchangedBook.Artwork = [_bookArtworkEntityFixture.Create(bookId: unchangedBook.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        context.Books.AddRange(changedBook, unchangedBook);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.ResetEnrichmentStateForPathsAsync(libraryId, ["/books/changed.epub"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        BookEntity? resetBook = await context.Books.AsNoTracking().Include(book => book.Artwork).FirstOrDefaultAsync(book => book.Id == changedBook.Id);
        Assert.NotNull(resetBook);
        Assert.Equal(MetadataStatus.Pending, resetBook!.MetadataStatus);
        Assert.Equal(ArtworkStatus.Pending, resetBook.Artwork.Single().Status);
        BookEntity? keptBook = await context.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == unchangedBook.Id);
        Assert.NotNull(keptBook);
        Assert.Equal(MetadataStatus.Enriched, keptBook!.MetadataStatus);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenMorePathsAreRequestedThanTheStorageAccepts_ShouldReturnTheExistingPaths()
    {
        // Arrange
        // A scan of a very large library stages more paths than SQLite accepts as the parameters of one statement, which used to fail the whole scan.
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-existingpaths-large-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.Path = "/books/tolkien/the-hobbit.epub";
        context.Books.Add(book);
        await context.SaveChangesAsync();
        List<string> requestedPaths = [.. Enumerable.Range(0, 120_000).Select(index => $"/books/tolkien/book-{index}.epub")];
        requestedPaths.Add("/books/tolkien/the-hobbit.epub");

        // Act
        Result<IReadOnlyCollection<string>> result = await sut.GetExistingPathsAsync(libraryId, requestedPaths, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        string existingPath = Assert.Single(result.Value);
        Assert.Equal("/books/tolkien/the-hobbit.epub", existingPath);
    }

    [Fact]
    public async Task ResetMetadataStatusForLibraryAsync_WhenCalled_ShouldResetTheMetadataStatusOfAllBooksOfTheLibrary()
    {
        // Arrange
        // The reset methods use ExecuteUpdateAsync, which is not supported by the in-memory provider, so a real SQLite database is used.
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-reset-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity enrichedBook = _bookEntityFixture.Create();
        enrichedBook.LibraryId = libraryId;
        enrichedBook.MetadataStatus = MetadataStatus.Enriched;
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.MetadataStatus = MetadataStatus.Enriched;
        context.Books.AddRange(enrichedBook, bookOfAnotherLibrary);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.ResetMetadataStatusForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        BookEntity? resetBook = await context.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == enrichedBook.Id);
        Assert.NotNull(resetBook);
        Assert.Equal(MetadataStatus.Pending, resetBook!.MetadataStatus);
        BookEntity? keptBook = await context.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == bookOfAnotherLibrary.Id);
        Assert.NotNull(keptBook);
        Assert.Equal(MetadataStatus.Enriched, keptBook!.MetadataStatus);
    }

    [Fact]
    public async Task ResetArtworkStatusForLibraryAsync_WhenCalled_ShouldResetTheArtworkStatusOfTheLibraryBooks()
    {
        // Arrange
        // The reset methods use ExecuteUpdateAsync, which is not supported by the in-memory provider, so a real SQLite database is used.
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-reset-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.Artwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.Artwork = [_bookArtworkEntityFixture.Create(bookId: bookOfAnotherLibrary.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        context.Books.AddRange(book, bookOfAnotherLibrary);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.ResetArtworkStatusForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        BookArtworkEntity? resetArtwork = await context.Set<BookArtworkEntity>().AsNoTracking().FirstOrDefaultAsync(artwork => artwork.BookId == book.Id);
        Assert.NotNull(resetArtwork);
        Assert.Equal(ArtworkStatus.Pending, resetArtwork!.Status);
        BookArtworkEntity? keptArtwork = await context.Set<BookArtworkEntity>().AsNoTracking().FirstOrDefaultAsync(artwork => artwork.BookId == bookOfAnotherLibrary.Id);
        Assert.NotNull(keptArtwork);
        Assert.Equal(ArtworkStatus.Enriched, keptArtwork!.Status);
    }

    [Fact]
    public async Task InsertAsync_WhenCalledWithAValidBook_ShouldPersistTheBook()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-insert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        BookEntity book = _bookEntityFixture.Create();

        // Act
        Result<Created> result = await sut.InsertAsync(book, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        BookEntity? storedBook = await context.Books.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == book.Id);
        Assert.NotNull(storedBook);
        Assert.Equal(book.Path, storedBook!.Path);
        Assert.Equal(book.Title, storedBook.Title);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherBookOfTheSameLibraryHasTheSamePath_ShouldReturnBookAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-insert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity existingBook = _bookEntityFixture.Create();
        existingBook.LibraryId = libraryId;
        existingBook.Path = "/books/existing.epub";
        context.Books.Add(existingBook);
        await context.SaveChangesAsync();

        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.Path = "/books/existing.epub";

        // Act
        Result<Created> result = await sut.InsertAsync(book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotChangeAnyAuditColumn()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-update-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        BookRepository sut = new(context);
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        book.Contributors = [_bookContributorEntityFixture.Create(bookId: book.Id)];
        book.Ratings = [_bookRatingEntityFixture.Create(source: BookRatingSource.GoogleBooks)];
        book.ISBNs = [_isbnEntityFixture.Create(value: "9783161484100", format: IsbnFormat.Isbn13)];
        context.Books.Add(book);
        await context.SaveChangesAsync();
        Guid contributorId = book.Contributors[0].Id;
        DateTime contributorCreatedOnUtc = book.Contributors[0].CreatedOnUtc;
        BookEntity incoming = await LoadDetachedBookAsync(context, book.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        BookEntity? storedBook = await context.Books.AsNoTracking().Include(candidate => candidate.Contributors).FirstOrDefaultAsync(candidate => candidate.Id == book.Id);
        Assert.NotNull(storedBook);
        Assert.Null(storedBook!.UpdatedOnUtc);
        BookContributorEntity storedContributor = Assert.Single(storedBook.Contributors);
        Assert.Equal(contributorId, storedContributor.Id);
        Assert.Equal(contributorCreatedOnUtc, storedContributor.CreatedOnUtc);
    }

    [Fact]
    public async Task UpdateAsync_WhenOnlyTheMetadataChanged_ShouldStampTheBookAndKeepTheChildRowsUntouched()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-update-metadata-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        BookRepository sut = new(context);
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        book.Contributors = [_bookContributorEntityFixture.Create(bookId: book.Id)];
        book.Ratings = [_bookRatingEntityFixture.Create(source: BookRatingSource.GoogleBooks)];
        book.ISBNs = [_isbnEntityFixture.Create(value: "9783161484100", format: IsbnFormat.Isbn13)];
        context.Books.Add(book);
        await context.SaveChangesAsync();
        Guid contributorId = book.Contributors[0].Id;
        DateTime contributorCreatedOnUtc = book.Contributors[0].CreatedOnUtc;
        BookEntity incoming = await LoadDetachedBookAsync(context, book.Id);
        incoming.Title = "A brand new title";

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        BookEntity? storedBook = await context.Books.AsNoTracking().Include(candidate => candidate.Contributors).FirstOrDefaultAsync(candidate => candidate.Id == book.Id);
        Assert.NotNull(storedBook);
        Assert.Equal("A brand new title", storedBook!.Title);
        Assert.Equal(utcNow, storedBook.UpdatedOnUtc);
        Assert.Equal(userId, storedBook.UpdatedBy);
        BookContributorEntity storedContributor = Assert.Single(storedBook.Contributors);
        Assert.Equal(contributorId, storedContributor.Id);
        Assert.Equal(contributorCreatedOnUtc, storedContributor.CreatedOnUtc);
    }

    [Fact]
    public async Task UpdateAsync_WhenAContributorIsRemoved_ShouldDeleteOnlyThatParticipationAndKeepTheOtherIdentity()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-update-removecontributor-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        BookRepository sut = new(context);
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        Guid keptMediaContributorId = Guid.NewGuid();
        BookContributorEntity keptContributor = _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: keptMediaContributorId);
        BookContributorEntity removedContributor = _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: Guid.NewGuid());
        book.Contributors = [keptContributor, removedContributor];
        context.Books.Add(book);
        await context.SaveChangesAsync();
        Guid keptContributorId = keptContributor.Id;
        BookEntity incoming = await LoadDetachedBookAsync(context, book.Id);
        incoming.Contributors = [incoming.Contributors.Single(contributor => contributor.MediaContributorId == keptMediaContributorId)];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<BookContributorEntity> storedContributors = await context.Set<BookContributorEntity>().AsNoTracking().Where(contributor => contributor.BookId == book.Id).ToListAsync();
        BookContributorEntity storedContributor = Assert.Single(storedContributors);
        Assert.Equal(keptContributorId, storedContributor.Id);
        Assert.Equal(keptMediaContributorId, storedContributor.MediaContributorId);
    }

    [Fact]
    public async Task UpdateAsync_WhenAContributorIsAdded_ShouldInsertOnlyTheNewParticipationAndKeepTheExistingIdentity()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-update-addcontributor-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        BookRepository sut = new(context);
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        Guid keptMediaContributorId = Guid.NewGuid();
        BookContributorEntity keptContributor = _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: keptMediaContributorId);
        book.Contributors = [keptContributor];
        context.Books.Add(book);
        await context.SaveChangesAsync();
        Guid keptContributorId = keptContributor.Id;
        BookEntity incoming = await LoadDetachedBookAsync(context, book.Id);
        Guid addedMediaContributorId = Guid.NewGuid();
        incoming.Contributors.Add(_bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: addedMediaContributorId));

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<BookContributorEntity> storedContributors = await context.Set<BookContributorEntity>().AsNoTracking().Where(contributor => contributor.BookId == book.Id).ToListAsync();
        Assert.Equal(2, storedContributors.Count);
        Assert.Equal(keptContributorId, storedContributors.Single(contributor => contributor.MediaContributorId == keptMediaContributorId).Id);
        Assert.Contains(storedContributors, contributor => contributor.MediaContributorId == addedMediaContributorId);
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotDuplicateAnyReconciledChildCollection()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-update-noop-collections-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        book.Tags = [_tagEntityFixture.Create(name: $"noop-book-tag-{Guid.NewGuid():N}"), _tagEntityFixture.Create(name: $"noop-book-tag-two-{Guid.NewGuid():N}")];
        book.Genres = [_genreEntityFixture.Create(name: $"noop-book-genre-{Guid.NewGuid():N}"), _genreEntityFixture.Create(name: $"noop-book-genre-two-{Guid.NewGuid():N}")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "9783161484100", format: IsbnFormat.Isbn13), _isbnEntityFixture.Create(value: "9780261102693", format: IsbnFormat.Isbn13)];
        book.Ratings = [_bookRatingEntityFixture.Create(source: BookRatingSource.GoogleBooks), _bookRatingEntityFixture.Create(source: BookRatingSource.Goodreads)];
        book.Contributors =
        [
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: Guid.NewGuid()),
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: Guid.NewGuid())
        ];
        context.Books.Add(book);
        await context.SaveChangesAsync();
        // Detach the seeded graph, so that the update must load the tracked book through its include chain instead of returning the already populated seeded instance.
        context.ChangeTracker.Clear();

        BookEntity incoming = await LoadDetachedBookAsync(context, book.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        BookEntity? storedBook = await context.Books
            .AsNoTracking()
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .Include(candidate => candidate.ISBNs)
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Contributors)
            .FirstOrDefaultAsync(candidate => candidate.Id == book.Id);
        Assert.NotNull(storedBook);
        Assert.Equal(2, storedBook!.Tags.Count);
        Assert.Equal(2, storedBook.Genres.Count);
        Assert.Equal(2, storedBook.ISBNs.Count);
        Assert.Equal(2, storedBook.Ratings.Count);
        Assert.Equal(2, storedBook.Contributors.Count);
    }

    [Fact]
    public async Task UpdateAsync_WhenOneItemIsRemovedFromEachReconciledChildCollection_ShouldRemoveOnlyThoseItems()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-update-remove-collections-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        string keptTagName = $"remove-book-tag-{Guid.NewGuid():N}";
        string keptGenreName = $"remove-book-genre-{Guid.NewGuid():N}";
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        book.Tags = [_tagEntityFixture.Create(name: keptTagName), _tagEntityFixture.Create(name: $"remove-book-tag-two-{Guid.NewGuid():N}")];
        book.Genres = [_genreEntityFixture.Create(name: keptGenreName), _genreEntityFixture.Create(name: $"remove-book-genre-two-{Guid.NewGuid():N}")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "9783161484100", format: IsbnFormat.Isbn13), _isbnEntityFixture.Create(value: "9780261102693", format: IsbnFormat.Isbn13)];
        book.Ratings = [_bookRatingEntityFixture.Create(source: BookRatingSource.GoogleBooks), _bookRatingEntityFixture.Create(source: BookRatingSource.Goodreads)];
        BookContributorEntity keptContributor = _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: Guid.NewGuid());
        book.Contributors = [keptContributor, _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: Guid.NewGuid())];
        context.Books.Add(book);
        await context.SaveChangesAsync();
        // Detach the seeded graph, so that the update must load the tracked book through its include chain instead of returning the already populated seeded instance.
        context.ChangeTracker.Clear();

        BookEntity incoming = await LoadDetachedBookAsync(context, book.Id);
        incoming.Tags = [incoming.Tags.Single(tag => tag.Name == keptTagName)];
        incoming.Genres = [incoming.Genres.Single(genre => genre.Name == keptGenreName)];
        incoming.ISBNs = [incoming.ISBNs.Single(isbn => isbn.Value == "9783161484100")];
        incoming.Ratings = [incoming.Ratings.Single(rating => rating.Source == BookRatingSource.GoogleBooks)];
        incoming.Contributors = [incoming.Contributors.Single(contributor => contributor.Id == keptContributor.Id)];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        BookEntity? storedBook = await context.Books
            .AsNoTracking()
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .Include(candidate => candidate.ISBNs)
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Contributors)
            .FirstOrDefaultAsync(candidate => candidate.Id == book.Id);
        Assert.NotNull(storedBook);
        Assert.Equal(keptTagName, Assert.Single(storedBook!.Tags).Name);
        Assert.Equal(keptGenreName, Assert.Single(storedBook.Genres).Name);
        Assert.Equal("9783161484100", Assert.Single(storedBook.ISBNs).Value);
        Assert.Equal(BookRatingSource.GoogleBooks, Assert.Single(storedBook.Ratings).Source);
        Assert.Equal(keptContributor.Id, Assert.Single(storedBook.Contributors).Id);
    }

    [Fact]
    public async Task DeleteByLibraryIdAsync_WhenCalled_ShouldDeleteOnlyThatLibrariesBooks()
    {
        // Arrange
        // The delete uses ExecuteDeleteAsync, which is not supported by the in-memory provider, so a real SQLite database is used.
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-deletebylibrary-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity bookOfLibrary = _bookEntityFixture.Create();
        bookOfLibrary.LibraryId = libraryId;
        bookOfLibrary.Path = "/books/library.epub";
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.Path = "/books/other.epub";
        context.Books.AddRange(bookOfLibrary, bookOfAnotherLibrary);
        await context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await sut.DeleteByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        context.ChangeTracker.Clear();
        BookEntity survivor = Assert.Single(await context.Books.AsNoTracking().ToListAsync());
        Assert.Equal(bookOfAnotherLibrary.Id, survivor.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenIdsIsEmpty_ShouldReturnAnEmptyList()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-getbyids-empty-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        context.Books.Add(_bookEntityFixture.Create(includeMetadata: false));
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await sut.GetByIdsAsync([], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenCalled_ShouldReturnOnlyTheRequestedBooks()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-getbyids-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        BookEntity firstBook = _bookEntityFixture.Create(includeMetadata: false);
        BookEntity secondBook = _bookEntityFixture.Create(includeMetadata: false);
        BookEntity thirdBook = _bookEntityFixture.Create(includeMetadata: false);
        context.Books.AddRange(firstBook, secondBook, thirdBook);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await sut.GetByIdsAsync([firstBook.Id, thirdBook.Id], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, book => book.Id == firstBook.Id);
        Assert.Contains(result.Value, book => book.Id == thirdBook.Id);
        Assert.DoesNotContain(result.Value, book => book.Id == secondBook.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNavigationPropertiesShouldNotBeIncluded_ShouldNotLoadTheRelatedCollections()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-getbyids-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        MediaContributorEntity author = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        book.Tags = [_tagEntityFixture.Create(name: "HiddenTag")];
        book.Genres = [_genreEntityFixture.Create(name: "HiddenGenre")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "9783161484100", format: IsbnFormat.Isbn13)];
        book.Ratings = [_bookRatingEntityFixture.Create(value: 4.5M, maxValue: 5, source: BookRatingSource.Goodreads, voteCount: 100)];
        book.Contributors = [_bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: author.Id, role: MediaContributorRole.Author)];
        book.Artwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        context.Books.Add(book);
        context.MediaContributors.Add(author);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await sut.GetByIdsAsync([book.Id], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity returnedBook = Assert.Single(result.Value);
        Assert.Equal(book.Id, returnedBook.Id);
        Assert.Empty(returnedBook.Tags);
        Assert.Empty(returnedBook.Genres);
        Assert.Empty(returnedBook.Contributors);
        Assert.Empty(returnedBook.Artwork);
        // ISBNs and Ratings are owned collections, so EF Core materializes them together with the book even when the optional graph is not
        // included. The flag only controls the non-owned navigation properties, which is why those are asserted empty above.
        Assert.Single(returnedBook.ISBNs);
        Assert.Single(returnedBook.Ratings);
        // The rows are read without tracking, so the read statistics never leak into the unit of work.
        Assert.Empty(context.ChangeTracker.Entries<BookEntity>());
        Assert.Equal(EntityState.Detached, context.Entry(returnedBook).State);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNavigationPropertiesShouldBeIncluded_ShouldLoadTheRelatedCollections()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-getbyids-include-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        MediaContributorEntity author = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        book.Tags = [_tagEntityFixture.Create(name: "LoadedTag"), _tagEntityFixture.Create(name: "SecondLoadedTag")];
        book.Genres = [_genreEntityFixture.Create(name: "LoadedGenre")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "9783161484100", format: IsbnFormat.Isbn13)];
        book.Ratings = [_bookRatingEntityFixture.Create(value: 4.5M, maxValue: 5, source: BookRatingSource.Goodreads, voteCount: 100)];
        book.Contributors = [_bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: author.Id, role: MediaContributorRole.Author)];
        book.Artwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched, fileName: "cover.jpg")];
        context.Books.Add(book);
        context.MediaContributors.Add(author);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await sut.GetByIdsAsync([book.Id], true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity returnedBook = Assert.Single(result.Value);
        Assert.Equal(2, returnedBook.Tags.Count);
        Assert.Single(returnedBook.Genres);
        Assert.Equal("9783161484100", Assert.Single(returnedBook.ISBNs).Value);
        Assert.Equal(4.5M, Assert.Single(returnedBook.Ratings).Value);
        Assert.Equal(author.Id, Assert.Single(returnedBook.Contributors).MediaContributorId);
        Assert.Equal("cover.jpg", Assert.Single(returnedBook.Artwork).FileName);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenExactlyFiveHundredIdsAreRequested_ShouldReturnEveryBook()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-getbyids-boundary-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        const int BOOK_COUNT = 500;
        List<BookEntity> books = CreateBooks(BOOK_COUNT);
        context.Books.AddRange(books);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        // A single full chunk proves the boundary does not drop the last chunk.
        Result<IReadOnlyList<BookEntity>> result = await sut.GetByIdsAsync([.. books.Select(book => book.Id)], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(BOOK_COUNT, result.Value.Count);
        Assert.Equal(
            books.Select(book => book.Id).OrderBy(id => id),
            result.Value.Select(book => book.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetByIdsAsync_WhenMoreThanFiveHundredIdsAreRequested_ShouldReturnEveryBook()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-getbyids-chunks-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        const int BOOK_COUNT = 501;
        List<BookEntity> books = CreateBooks(BOOK_COUNT);
        context.Books.AddRange(books);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        // The lookup is chunked in batches of 500, so the 501st Id proves the second chunk is queried as well.
        Result<IReadOnlyList<BookEntity>> result = await sut.GetByIdsAsync([.. books.Select(book => book.Id)], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(BOOK_COUNT, result.Value.Count);
        Assert.Equal(
            books.Select(book => book.Id).OrderBy(id => id),
            result.Value.Select(book => book.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenBookHasMultipleAuthors_ShouldReturnTheMinimumAuthorDisplayName()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-authors-min-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        MediaContributorEntity firstAuthor = _mediaContributorEntityFixture.Create(displayName: "Zoe Author");
        MediaContributorEntity secondAuthor = _mediaContributorEntityFixture.Create(displayName: "Alice Author");
        MediaContributorEntity translator = _mediaContributorEntityFixture.Create(displayName: "Aaron Translator");
        context.Books.Add(book);
        context.MediaContributors.AddRange(firstAuthor, secondAuthor, translator);
        await context.SaveChangesAsync();
        context.BookContributors.AddRange(
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: firstAuthor.Id, role: MediaContributorRole.Author),
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: secondAuthor.Id, role: MediaContributorRole.Author),
            // The translator sorts before every author, so the result would change if a non-Author role were included.
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: translator.Id, role: MediaContributorRole.Translator));
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyDictionary<Guid, string?>> result = await sut.GetAuthorsDisplayNamesByBookIdsAsync([book.Id], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Alice Author", result.Value[book.Id]);
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenABookIsNotRequested_ShouldNotReturnIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-authors-requested-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        BookEntity requestedBook = _bookEntityFixture.Create(includeMetadata: false);
        BookEntity bookNotRequested = _bookEntityFixture.Create(includeMetadata: false);
        MediaContributorEntity requestedAuthor = _mediaContributorEntityFixture.Create(displayName: "Requested Author");
        MediaContributorEntity otherAuthor = _mediaContributorEntityFixture.Create(displayName: "Other Author");
        context.Books.AddRange(requestedBook, bookNotRequested);
        context.MediaContributors.AddRange(requestedAuthor, otherAuthor);
        await context.SaveChangesAsync();
        context.BookContributors.AddRange(
            _bookContributorEntityFixture.Create(bookId: requestedBook.Id, mediaContributorId: requestedAuthor.Id, role: MediaContributorRole.Author),
            _bookContributorEntityFixture.Create(bookId: bookNotRequested.Id, mediaContributorId: otherAuthor.Id, role: MediaContributorRole.Author));
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyDictionary<Guid, string?>> result = await sut.GetAuthorsDisplayNamesByBookIdsAsync([requestedBook.Id], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Requested Author", result.Value[requestedBook.Id]);
        Assert.DoesNotContain(bookNotRequested.Id, result.Value.Keys);
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenBookIdsIsEmpty_ShouldReturnAnEmptyDictionary()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-authors-empty-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        MediaContributorEntity author = _mediaContributorEntityFixture.Create();
        context.Books.Add(book);
        context.MediaContributors.Add(author);
        await context.SaveChangesAsync();
        context.BookContributors.Add(
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: author.Id, role: MediaContributorRole.Author));
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyDictionary<Guid, string?>> result = await sut.GetAuthorsDisplayNamesByBookIdsAsync([], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenMoreThanFiveHundredIdsAreRequested_ShouldReturnTheAuthorOfEveryRequestedBook()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-authors-chunks-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        BookRepository sut = new(context);
        const int BOOK_COUNT = 501;
        List<BookEntity> books = CreateBooks(BOOK_COUNT);
        MediaContributorEntity author = _mediaContributorEntityFixture.Create(displayName: "Shared Author");
        context.Books.AddRange(books);
        context.MediaContributors.Add(author);
        await context.SaveChangesAsync();
        context.BookContributors.AddRange([.. books.Select(book =>
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: author.Id, role: MediaContributorRole.Author))]);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        // The lookup is chunked in batches of 500, so the 501st book proves the second chunk is queried as well.
        Result<IReadOnlyDictionary<Guid, string?>> result = await sut.GetAuthorsDisplayNamesByBookIdsAsync(
            [.. books.Select(book => book.Id)], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(BOOK_COUNT, result.Value.Count);
        Assert.All(result.Value.Keys, bookId => Assert.Equal("Shared Author", result.Value[bookId]));
    }

    /// <summary>
    /// Creates a list of books with distinct paths and empty metadata collections, so that the chunked lookups can be exercised without extra graph data.
    /// </summary>
    /// <param name="count">The number of books to create.</param>
    /// <returns>The created books.</returns>
    private List<BookEntity> CreateBooks(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => _bookEntityFixture.Create(
            path: $"/books/generated-{Guid.NewGuid():N}.epub", includeMetadata: false))];
    }

    /// <summary>
    /// Creates a real SQLite backed context, so that the stored values are read exactly as they were written.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <returns>The created context.</returns>
    private static LuminaDbContext CreateContext(SqliteConnection anchorConnection)
    {
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a real SQLite backed context with the auditing interceptor attached, so that audit columns behave exactly like in production.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <param name="userId">The Id of the user reported as the current user.</param>
    /// <param name="utcNow">The current UTC time reported by the time provider.</param>
    /// <returns>The created context.</returns>
    private static LuminaDbContext CreateAuditedContext(SqliteConnection anchorConnection, Guid userId, DateTime utcNow)
    {
        ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(utcNow);
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .AddInterceptors(new UpdateAuditableEntitiesInterceptor(currentUserService, dateTimeProvider))
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Loads a detached copy of a stored book, with its children, so that it can be handed to the repository as the desired state of an edit.
    /// </summary>
    /// <param name="context">The context that tracks the stored book.</param>
    /// <param name="bookId">The Id of the book to load.</param>
    /// <returns>The detached copy of the book.</returns>
    private static async Task<BookEntity> LoadDetachedBookAsync(LuminaDbContext context, Guid bookId)
    {
        return await context.Books
            .AsNoTracking()
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .Include(book => book.Ratings)
            .Include(book => book.Contributors)
            .FirstAsync(book => book.Id == bookId);
    }
}
