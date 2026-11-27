#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.Management.Deletion;
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
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
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMediaLibraryItemDeletionStrategy _mockDeletionStrategy;
    private readonly ILogger<LibraryMediaItemDeletedDomainEventHandler> _mockLogger;
    private readonly LibraryMediaItemDeletedDomainEventHandler _sut;
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryMediaItemDeletedDomainEventFixture _libraryMediaItemDeletedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMediaItemDeletedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryMediaItemDeletedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);

        _mockDeletionStrategy = Substitute.For<IMediaLibraryItemDeletionStrategy>();
        _mockLogger = Substitute.For<ILogger<LibraryMediaItemDeletedDomainEventHandler>>();

        _sut = new LibraryMediaItemDeletedDomainEventHandler(_mockUnitOfWork, [_mockDeletionStrategy], _mockLogger);
    }

    [Fact]
    public async Task HandleAsync_WhenDeletionStrategyExistsForTheLibraryType_ShouldDelegateToIt()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, libraryType: LibraryType.Book);
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockDeletionStrategy.SupportedLibraryType.Returns(LibraryType.Book);
        _mockDeletionStrategy.DeleteItemAsync(libraryId.Value, "/books/deleted.epub", Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: "/books/deleted.epub");

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockDeletionStrategy.Received(1).DeleteItemAsync(libraryId.Value, "/books/deleted.epub", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoDeletionStrategyExistsForTheLibraryType_ShouldNotDelete()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, libraryType: LibraryType.Music);
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockDeletionStrategy.SupportedLibraryType.Returns(LibraryType.Book);

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: "/music/deleted.flac");

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockDeletionStrategy.DidNotReceive().DeleteItemAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetLibraryFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the library");
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: "/books/deleted.epub");

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockDeletionStrategy.DidNotReceive().DeleteItemAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: "/books/deleted.epub");

        // Act & Assert
        await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());
        await _mockDeletionStrategy.DidNotReceive().DeleteItemAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletionFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId.Value, libraryType: LibraryType.Book);
        Error error = Error.Failure("Database.Error", "Failed to delete the item");
        _mockLibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockDeletionStrategy.SupportedLibraryType.Returns(LibraryType.Book);
        _mockDeletionStrategy.DeleteItemAsync(libraryId.Value, "/books/deleted.epub", Arg.Any<CancellationToken>())
            .Returns(error);

        LibraryMediaItemDeletedDomainEvent domainEvent = _libraryMediaItemDeletedDomainEventFixture.Create(libraryId: libraryId, path: "/books/deleted.epub");

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
    }
}
