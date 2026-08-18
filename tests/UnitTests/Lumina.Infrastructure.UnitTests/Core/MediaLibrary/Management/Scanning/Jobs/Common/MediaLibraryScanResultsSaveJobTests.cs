#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanResultsSaveJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanResultsSaveJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanSnapshotRepository _mockSnapshotRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly MediaLibraryScanResultsSaveJob _sut;
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanResultsSaveJobTests"/> class.
    /// </summary>
    public MediaLibraryScanResultsSaveJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockSnapshotRepository = Substitute.For<ILibraryScanSnapshotRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork.LibraryScanSnapshotRepository.Returns(_mockSnapshotRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();
        _sut = new MediaLibraryScanResultsSaveJob(_mockServiceScopeFactory)
        {
            ScanId = _scanId,
            UserId = _userId,
            LibraryId = _libraryId
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenScanResultsAreApplied_ShouldCreateShellBooksAndPublishFinishedEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["deleted.pdf"]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["book1.pdf", "book2.pdf"]));
        _mockBookRepository.GetByPathAsync(_libraryId.Value, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Created));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryMediaItemDeletedDomainEvent>(domainEvent => domainEvent.Path == "deleted.pdf"), Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).InsertAsync(Arg.Is<BookEntity>(book => book.Path == "book1.pdf" && book.Title == "book1" && book.MetadataStatus == MetadataStatus.Pending), Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).InsertAsync(Arg.Is<BookEntity>(book => book.Path == "book2.pdf" && book.Title == "book2" && book.MetadataStatus == MetadataStatus.Pending), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFinishedDomainEvent>(domainEvent => domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenBookAlreadyExists_ShouldSkipInsertingIt()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["existing.pdf"]));
        _mockBookRepository.GetByPathAsync(_libraryId.Value, "existing.pdf", Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(_bookEntityFixture.Create(path: "existing.pdf")));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingDeletedPathsFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the deleted paths"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldMarkJobAsCanceledAndThrow()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task operation = _sut.ExecuteAsync(Guid.NewGuid(), new { }, cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        Assert.Equal(LibraryScanJobStatus.Canceled, _sut.Status);
    }
}
