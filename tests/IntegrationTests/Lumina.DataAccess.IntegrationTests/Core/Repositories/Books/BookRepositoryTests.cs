#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.UoW;
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

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Books;

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
        changedBook.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: changedBook.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity unchangedBook = _bookEntityFixture.Create();
        unchangedBook.LibraryId = libraryId;
        unchangedBook.Path = "/books/unchanged.epub";
        unchangedBook.MetadataStatus = MetadataStatus.Enriched;
        unchangedBook.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: unchangedBook.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        context.Books.AddRange(changedBook, unchangedBook);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.ResetEnrichmentStateForPathsAsync(libraryId, ["/books/changed.epub"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        BookEntity? resetBook = await context.Books.AsNoTracking().Include(book => book.BookArtwork).FirstOrDefaultAsync(book => book.Id == changedBook.Id);
        Assert.NotNull(resetBook);
        Assert.Equal(MetadataStatus.Pending, resetBook!.MetadataStatus);
        Assert.Equal(ArtworkStatus.Pending, resetBook.BookArtwork.Single().Status);
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
        book.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: bookOfAnotherLibrary.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
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
}
