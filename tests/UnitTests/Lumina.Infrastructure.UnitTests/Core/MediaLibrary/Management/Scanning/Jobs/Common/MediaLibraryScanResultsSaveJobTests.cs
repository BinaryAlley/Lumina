#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ILibraryScanSnapshotRepository _mockSnapshotRepository;
    private readonly ILibraryScanStagingResultsRepository _mockStagingResultsRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly MediaLibraryScanResultsSaveJob _sut;
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
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
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockSnapshotRepository = Substitute.For<ILibraryScanSnapshotRepository>();
        _mockStagingResultsRepository = Substitute.For<ILibraryScanStagingResultsRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.LibraryScanSnapshotRepository.Returns(_mockSnapshotRepository);
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockStagingResultsRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        // By default no book exists yet, so every scanned path is materialized; the tests that need existing books override this.
        _mockBookRepository.GetExistingPathsAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>([]));
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);
        // The job resolves the materializer of the loaded library type from the service scope, so the books materializer must be available.
        ILibraryPathTemplateService mockPathTemplateService = Substitute.For<ILibraryPathTemplateService>();
        mockPathTemplateService.ResolveTemplate(Arg.Any<LibraryType>(), Arg.Any<IReadOnlyList<LibraryPathPart>>()).Returns(Result.From(LibraryPathTemplate.Empty()));
        IPathService mockPathService = Substitute.For<IPathService>();
        mockPathService.GetFileName(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());
        mockPathService.GetFileNameWithoutExtension(Arg.Any<string>()).Returns(callInfo =>
        {
            string fileName = callInfo.Arg<string>();
            int lastDotIndex = fileName.LastIndexOf('.');
            return lastDotIndex <= 0 ? fileName : fileName[..lastDotIndex];
        });
        _mockServiceProvider.GetService(typeof(IEnumerable<IMediaLibraryScanItemMaterializer>))
            .Returns(new IMediaLibraryScanItemMaterializer[] { new BooksMediaLibraryScanItemMaterializer(mockPathTemplateService, mockPathService) });

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(id: _libraryId.Value, libraryType: LibraryType.Book)));
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
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["changed.pdf"]));
        _mockBookRepository.ResetEnrichmentStateForPathsAsync(_libraryId.Value, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["book1.pdf", "book2.pdf"]));
        _mockBookRepository.GetExistingPathsAsync(_libraryId.Value, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>([]));
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Created));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryMediaItemDeletedDomainEvent>(domainEvent => domainEvent.Path == "deleted.pdf"), Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).ResetEnrichmentStateForPathsAsync(_libraryId.Value, Arg.Is<IReadOnlyCollection<string>>(paths => paths.Any(path => path == "changed.pdf")), Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).InsertAsync(Arg.Is<BookEntity>(book => book.Path == "book1.pdf" && book.Title == "book1" && book.MetadataStatus == MetadataStatus.Pending), Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).InsertAsync(Arg.Is<BookEntity>(book => book.Path == "book2.pdf" && book.Title == "book2" && book.MetadataStatus == MetadataStatus.Pending), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFinishedDomainEvent>(domainEvent => domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenScanResultsAreApplied_ShouldPublishProgressChangedEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanProgressChangedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoContentChanged_ShouldNotResetTheEnrichmentState()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // No book content changed since the last scan, so the enrichment state of the books must not be reset.
        await _mockBookRepository.DidNotReceive().ResetEnrichmentStateForPathsAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBookAlreadyExists_ShouldSkipInsertingIt()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["existing.pdf"]));
        _mockBookRepository.GetExistingPathsAsync(_libraryId.Value, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>(["existing.pdf"]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

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

    [Fact]
    public async Task ExecuteAsync_WhenGettingChangedPathsFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the changed paths"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockSnapshotRepository.DidNotReceive().ApplySnapshotSwapAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApplyingSnapshotSwapFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to apply the snapshot swap"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenResettingEnrichmentStateFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["changed.pdf"]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockBookRepository.ResetEnrichmentStateForPathsAsync(_libraryId.Value, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to reset the enrichment state"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingPathsFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the paths"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingExistingPathsFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["book1.pdf"]));
        _mockBookRepository.GetExistingPathsAsync(_libraryId.Value, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the existing paths"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenInsertingShellBookFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["book1.pdf"]));
        _mockBookRepository.GetExistingPathsAsync(_libraryId.Value, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>([]));
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to insert the shell book"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenJobHasLinkedChildren_ShouldExecuteEachChildWithoutPublishingFinishedEvent()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        IMediaLibraryScanJob mockChild = Substitute.For<IMediaLibraryScanJob>();
        mockChild.ExecuteAsync(Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _sut.AddChild(mockChild);
        object input = new();

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), input, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await mockChild.Received(1).ExecuteAsync(Arg.Any<Guid>(), Arg.Is<object>(executedInput => executedInput == input), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<LibraryScanFinishedDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenBookPathYieldsEmptyDerivedTitle_ShouldFallBackToTheFileNameAsTitle()
    {
        // Arrange
        _mockSnapshotRepository.GetDeletedPathsAsync(_libraryId.Value, _scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockStagingResultsRepository.GetChangedPathsAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([]));
        _mockSnapshotRepository.ApplySnapshotSwapAsync(_libraryId.Value, _scanId.Value, _userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockSnapshotRepository.GetPathsAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(["_.pdf"]));
        _mockBookRepository.GetExistingPathsAsync(_libraryId.Value, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>([]));
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Created));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // The file name is made only of separators, so the derived title is empty and the raw file name is used as a fallback.
        await _mockBookRepository.Received(1).InsertAsync(Arg.Is<BookEntity>(book => book.Path == "_.pdf" && book.Title == "_"), Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }
}
