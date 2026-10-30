#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Contains unit tests for the <see cref="BooksFileSystemDiscoveryJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksFileSystemDiscoveryJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ILibraryScanStagingResultsRepository _mockStagingResultsRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly ILogger<BooksFileSystemDiscoveryJob> _mockLogger;
    private readonly BooksFileSystemDiscoveryJob _sut;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksFileSystemDiscoveryJobTests"/> class.
    /// </summary>
    public BooksFileSystemDiscoveryJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockStagingResultsRepository = Substitute.For<ILibraryScanStagingResultsRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockStagingResultsRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();
        _mockLogger = Substitute.For<ILogger<BooksFileSystemDiscoveryJob>>();
        _mockLogger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _sut = new BooksFileSystemDiscoveryJob(_mockServiceScopeFactory, _mockLogger)
        {
            ScanId = _scanId,
            UserId = _userId,
            LibraryId = _libraryId
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingTheLibraryFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to retrieve the library"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockStagingResultsRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryDoesNotExist_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
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
    public async Task ExecuteAsync_WhenTheLibraryIsOnSsd_ShouldLogThatTheFileSystemReadsRunInParallel()
    {
        // Arrange
        string temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), $"lumina-books-ssd-{Guid.NewGuid()}");
        Directory.CreateDirectory(temporaryDirectoryPath);
        File.WriteAllText(Path.Combine(temporaryDirectoryPath, "book.pdf"), "book content");
        try
        {
            LibraryEntity libraryEntity = _libraryEntityFixture.Create(
                id: _libraryId.Value,
                userId: _userId.Value,
                libraryType: LibraryType.Book,
                contentLocations: [temporaryDirectoryPath],
                isContentOnSsd: true);
            _mockLibraryRepository.GetByIdAsync(_libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result.From<LibraryEntity?>(libraryEntity));
            _mockStagingResultsRepository.InsertRangeAsync(Arg.Any<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(Result.Created));

            // Act
            await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

            // Assert
            Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
            await _mockStagingResultsRepository.Received(1).InsertRangeAsync(
                Arg.Is<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(entities => entities.Count == 1 && Path.GetFileName(entities.First().Path) == "book.pdf"),
                Arg.Any<CancellationToken>());
            _mockLogger.Received(1).Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains("solid state drive", StringComparison.Ordinal)),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
        finally
        {
            Directory.Delete(temporaryDirectoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task StageFilesInParallelAsync_WhenAFileVanishesBeforeItsMetadataIsRead_ShouldSkipItAndLogAWarning()
    {
        // Arrange
        FileInfo vanishedFile = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "vanished.pdf"));
        List<LibraryScanStagingResultsEntity> stagingBatch = [];

        // Act
        await _sut.StageFilesInParallelAsync([vanishedFile], stagingBatch, _mockStagingResultsRepository, CancellationToken.None);

        // Assert
        Assert.Empty(stagingBatch);
        await _mockStagingResultsRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(), Arg.Any<CancellationToken>());
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("vanished.pdf", StringComparison.Ordinal)),
            Arg.Any<IOException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task StageFilesInParallelAsync_WhenAFileReadThrowsAnUnexpectedException_ShouldRethrowTheRealCause()
    {
        // Arrange
        List<LibraryScanStagingResultsEntity> stagingBatch = [];
        List<FileInfo> files = [null!];

        // Act
        Exception exception = await Assert.ThrowsAsync<NullReferenceException>(() =>
            _sut.StageFilesInParallelAsync(files, stagingBatch, _mockStagingResultsRepository, CancellationToken.None));

        // Assert
        // Parallel.For wraps the body exception in an AggregateException, which would otherwise be reported as "One or more errors occurred.".
        Assert.IsType<NullReferenceException>(exception);
    }
}
