#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.Management.Deletion;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Deletion;

/// <summary>
/// Contains unit tests for the <see cref="BooksMediaLibraryItemDeletionStrategy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksMediaLibraryItemDeletionStrategyTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IBookArtworkService _mockBookArtworkService;
    private readonly ILogger<BooksMediaLibraryItemDeletionStrategy> _mockLogger;
    private readonly BooksMediaLibraryItemDeletionStrategy _sut;
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksMediaLibraryItemDeletionStrategyTests"/> class.
    /// </summary>
    public BooksMediaLibraryItemDeletionStrategyTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);

        _mockBookArtworkService = Substitute.For<IBookArtworkService>();
        _mockLogger = Substitute.For<ILogger<BooksMediaLibraryItemDeletionStrategy>>();

        _sut = new BooksMediaLibraryItemDeletionStrategy(_mockUnitOfWork, _mockBookArtworkService, _mockLogger);
    }

    [Fact]
    public async Task DeleteItemAsync_WhenBookExistsAtPath_ShouldDeleteTheBookAndItsArtwork()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        BookEntity book = _bookEntityFixture.Create(path: "/books/deleted.epub");
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, title: "My Library");
        _mockBookRepository.GetByPathAsync(libraryId.Value, book.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));
        _mockBookRepository.DeleteByIdAsync(book.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockBookArtworkService.DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Deleted);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, book.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockBookArtworkService.Received(1).DeleteBookArtwork(libraryId.Value, book.Id, "My Library", "Frank Herbert", book.Title);
        await _mockBookRepository.Received(1).DeleteByIdAsync(book.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenNoBookExistsAtPath_ShouldDoNothing()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        _mockBookRepository.GetByPathAsync(libraryId.Value, "/books/deleted.epub", Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, "/books/deleted.epub", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockBookArtworkService.DidNotReceive().DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockBookRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenDeleteArtworkFails_ShouldStillDeleteTheBook()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        BookEntity book = _bookEntityFixture.Create(path: "/books/deleted.epub");
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, title: "My Library");
        _mockBookRepository.GetByPathAsync(libraryId.Value, book.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));
        _mockBookArtworkService.DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Error.Failure("Artwork.DeleteFailed", "Failed to delete the stored artwork"));
        _mockBookRepository.DeleteByIdAsync(book.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, book.Path, CancellationToken.None);

        // Assert
        // a failing artwork deletion must not prevent the book from being removed, so the deletion is only logged
        Assert.True(result.IsSuccess);
        await _mockBookRepository.Received(1).DeleteByIdAsync(book.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenGetBookFails_ShouldReturnFailure()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the book");
        _mockBookRepository.GetByPathAsync(libraryId.Value, "/books/deleted.epub", Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, "/books/deleted.epub", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockBookRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenDeleteBookFails_ShouldReturnFailure()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        BookEntity book = _bookEntityFixture.Create(path: "/books/deleted.epub");
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, title: "My Library");
        _mockBookRepository.GetByPathAsync(libraryId.Value, book.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));
        _mockBookArtworkService.DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Deleted);
        Error error = Error.Failure("Database.Error", "Failed to delete the book");
        _mockBookRepository.DeleteByIdAsync(book.Id, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, book.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
