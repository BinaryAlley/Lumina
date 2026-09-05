#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryMediaItemDeletedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMediaItemDeletedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IBookArtworkService _mockBookArtworkService;
    private readonly ILogger<LibraryMediaItemDeletedDomainEventHandler> _mockLogger;
    private readonly LibraryMediaItemDeletedDomainEventHandler _sut;
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryMediaItemDeletedDomainEventFixture _libraryMediaItemDeletedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMediaItemDeletedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryMediaItemDeletedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);

        _mockBookArtworkService = Substitute.For<IBookArtworkService>();
        _mockLogger = Substitute.For<ILogger<LibraryMediaItemDeletedDomainEventHandler>>();

        _sut = new LibraryMediaItemDeletedDomainEventHandler(_mockUnitOfWork, _mockBookArtworkService, _mockLogger);
    }

    [Fact]
    public async Task HandleAsync_WhenBookExistsAtPath_ShouldDeleteTheBookAndItsArtwork()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        BookEntity book = _bookEntityFixture.Create(path: "/books/deleted.epub");
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, title: "My Library");
        _mockBookRepository.GetByPathAsync(libraryId.Value, book.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));
        _mockBookRepository.DeleteAsync(book.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockBookArtworkService.DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Deleted);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: book.Path);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockBookArtworkService.Received(1).DeleteBookArtwork(libraryId.Value, book.Id, "My Library", "Frank Herbert", book.Title);
        await _mockBookRepository.Received(1).DeleteAsync(book.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoBookExistsAtPath_ShouldDoNothing()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        _mockBookRepository.GetByPathAsync(libraryId.Value, "/books/deleted.epub", Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: "/books/deleted.epub");

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockBookArtworkService.DidNotReceive().DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockBookRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteArtworkFails_ShouldStillDeleteTheBook()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        BookEntity book = _bookEntityFixture.Create(path: "/books/deleted.epub");
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, title: "My Library");
        _mockBookRepository.GetByPathAsync(libraryId.Value, book.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));
        _mockBookArtworkService.DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Error.Failure("Artwork.DeleteFailed", "Failed to delete the stored artwork"));
        _mockBookRepository.DeleteAsync(book.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: book.Path);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        // a failing artwork deletion must not prevent the book from being removed, so the deletion is only logged
        await _mockBookRepository.Received(1).DeleteAsync(book.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetBookFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the book");
        _mockBookRepository.GetByPathAsync(libraryId.Value, "/books/deleted.epub", Arg.Any<CancellationToken>())
            .Returns(error);

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: "/books/deleted.epub");

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockBookRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteBookFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        BookEntity book = _bookEntityFixture.Create(path: "/books/deleted.epub");
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, title: "My Library");
        _mockBookRepository.GetByPathAsync(libraryId.Value, book.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [book.Id] = "Frank Herbert" }));
        _mockBookArtworkService.DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Deleted);
        Error error = Error.Failure("Database.Error", "Failed to delete the book");
        _mockBookRepository.DeleteAsync(book.Id, Arg.Any<CancellationToken>())
            .Returns(error);

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: book.Path);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
