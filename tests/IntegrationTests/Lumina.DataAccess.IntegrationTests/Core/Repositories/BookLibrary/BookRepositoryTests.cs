#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.Repositories.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
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
}
