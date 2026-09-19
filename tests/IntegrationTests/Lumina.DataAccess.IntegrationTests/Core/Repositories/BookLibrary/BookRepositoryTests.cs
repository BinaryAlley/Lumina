#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
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

    [Fact]
    public async Task ResetEnrichmentStateForPathsAsync_WhenCalled_ShouldResetTheMetadataAndArtworkStatusesForThePaths()
    {
        // Arrange
        // The reset methods use ExecuteUpdateAsync, which is not supported by the in-memory provider, so a real SQLite database is used.
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-reset-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
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
    public async Task ResetMetadataStatusForLibraryAsync_WhenCalled_ShouldResetTheMetadataStatusOfAllBooksOfTheLibrary()
    {
        // Arrange
        // The reset methods use ExecuteUpdateAsync, which is not supported by the in-memory provider, so a real SQLite database is used.
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-reset-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
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
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
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
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
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
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
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
