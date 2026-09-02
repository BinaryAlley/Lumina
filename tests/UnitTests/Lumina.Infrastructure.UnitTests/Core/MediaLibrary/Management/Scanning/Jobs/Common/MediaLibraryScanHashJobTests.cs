#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
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
/// Contains unit tests for the <see cref="MediaLibraryScanHashJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanHashJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanStagingResultsRepository _mockStagingResultsRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly IFileHashService _mockFileHashService;
    private readonly MediaLibraryScanHashJob _sut;
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly HashedFileSystemFileDtoFixture _hashedFileSystemFileDtoFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanHashJobTests"/> class.
    /// </summary>
    public MediaLibraryScanHashJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockStagingResultsRepository = Substitute.For<ILibraryScanStagingResultsRepository>();
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockStagingResultsRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        _mockFileHashService = Substitute.For<IFileHashService>();
        _mockServiceProvider.GetService(typeof(IFileHashService)).Returns(_mockFileHashService);

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();
        _sut = new MediaLibraryScanHashJob(_mockServiceScopeFactory)
        {
            ScanId = _scanId,
            UserId = _userId,
            LibraryId = _libraryId
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenFilesNeedHashing_ShouldHashAndPersistThemAndComplete()
    {
        // Arrange
        List<HashedFileSystemFileDto> filesToHash = [_hashedFileSystemFileDtoFixture.Create(path: "/books/book1.pdf"), _hashedFileSystemFileDtoFixture.Create(path: "/books/book2.pdf")];
        _mockStagingResultsRepository.GetFilesToHashCountAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(2));
        _mockStagingResultsRepository.GetFilesToHashPageAsync(_scanId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<IReadOnlyList<HashedFileSystemFileDto>>(filesToHash),
                Result.From<IReadOnlyList<HashedFileSystemFileDto>>([]));
        _mockFileHashService.HashFilesAsync(Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<IReadOnlyCollection<HashedFileSystemFileDto>>()
                .Select(file => file with { CurrentHash = 42UL })
                .ToList()));
        _mockStagingResultsRepository.UpdateFileHashesAsync(_scanId.Value, Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockFileHashService.Received(1).HashFilesAsync(
            Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(),
            Arg.Any<Func<Task>>(),
            Arg.Any<CancellationToken>());
        await _mockStagingResultsRepository.Received(1).UpdateFileHashesAsync(
            _scanId.Value,
            Arg.Is<IReadOnlyCollection<HashedFileSystemFileDto>>(files => files.Count == 2 && files.All(file => file.CurrentHash == 42UL)),
            Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanProgressChangedDomainEvent>(domainEvent => domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFilesNeedHashing_ShouldCompleteWithoutHashing()
    {
        // Arrange
        _mockStagingResultsRepository.GetFilesToHashCountAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(0));
        _mockStagingResultsRepository.GetFilesToHashPageAsync(_scanId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<HashedFileSystemFileDto>>([]));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockFileHashService.DidNotReceive().HashFilesAsync(Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
        await _mockStagingResultsRepository.DidNotReceive().UpdateFileHashesAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanProgressChangedDomainEvent>(domainEvent => domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCountingFilesFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockStagingResultsRepository.GetFilesToHashCountAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to count the files to hash"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockFileHashService.DidNotReceive().HashFilesAsync(Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
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
    public async Task ExecuteAsync_WhenGettingTheFilesToHashPageFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockStagingResultsRepository.GetFilesToHashCountAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockStagingResultsRepository.GetFilesToHashPageAsync(_scanId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the files to hash"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockFileHashService.DidNotReceive().HashFilesAsync(Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUpdatingTheFileHashesFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        List<HashedFileSystemFileDto> filesToHash = [_hashedFileSystemFileDtoFixture.Create(path: "/books/book1.pdf"), _hashedFileSystemFileDtoFixture.Create(path: "/books/book2.pdf")];
        _mockStagingResultsRepository.GetFilesToHashCountAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(2));
        _mockStagingResultsRepository.GetFilesToHashPageAsync(_scanId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<IReadOnlyList<HashedFileSystemFileDto>>(filesToHash),
                Result.From<IReadOnlyList<HashedFileSystemFileDto>>([]));
        _mockFileHashService.HashFilesAsync(Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<IReadOnlyCollection<HashedFileSystemFileDto>>()
                .Select(file => file with { CurrentHash = 42UL })
                .ToList()));
        _mockStagingResultsRepository.UpdateFileHashesAsync(_scanId.Value, Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to update the file hashes"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockFileHashService.Received(1).HashFilesAsync(Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheHashingCallbackRunsBeforeTheProgressInterval_ShouldNotPublishJobProgressFromTheCallback()
    {
        // Arrange
        List<HashedFileSystemFileDto> filesToHash = [_hashedFileSystemFileDtoFixture.Create(path: "/books/book1.pdf")];
        _mockStagingResultsRepository.GetFilesToHashCountAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockStagingResultsRepository.GetFilesToHashPageAsync(_scanId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<IReadOnlyList<HashedFileSystemFileDto>>(filesToHash),
                Result.From<IReadOnlyList<HashedFileSystemFileDto>>([]));
        _mockFileHashService.HashFilesAsync(Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                await callInfo.Arg<Func<Task>>()();
                return callInfo.Arg<IReadOnlyCollection<HashedFileSystemFileDto>>()
                    .Select(file => file with { CurrentHash = 42UL })
                    .ToList();
            });
        _mockStagingResultsRepository.UpdateFileHashesAsync(_scanId.Value, Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockStagingResultsRepository.Received(1).UpdateFileHashesAsync(
            _scanId.Value,
            Arg.Is<IReadOnlyCollection<HashedFileSystemFileDto>>(files => files.All(file => file.CurrentHash == 42UL)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheHashingCallbackRunsAfterTheProgressInterval_ShouldPublishJobProgressFromTheCallback()
    {
        // Arrange
        List<HashedFileSystemFileDto> filesToHash = [_hashedFileSystemFileDtoFixture.Create(path: "/books/book1.pdf")];
        _mockStagingResultsRepository.GetFilesToHashCountAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockStagingResultsRepository.GetFilesToHashPageAsync(_scanId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<IReadOnlyList<HashedFileSystemFileDto>>(filesToHash),
                Result.From<IReadOnlyList<HashedFileSystemFileDto>>([]));
        _mockFileHashService.HashFilesAsync(Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                await Task.Delay(150);
                await callInfo.Arg<Func<Task>>()();
                return callInfo.Arg<IReadOnlyCollection<HashedFileSystemFileDto>>()
                    .Select(file => file with { CurrentHash = 42UL })
                    .ToList();
            });
        _mockStagingResultsRepository.UpdateFileHashesAsync(_scanId.Value, Arg.Any<IReadOnlyCollection<HashedFileSystemFileDto>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        // the initial progress and the progress published from the hashing callback are both reported
        await _mockDomainEventPublisher.Received(2).PublishAsync(Arg.Is<LibraryScanJobProgressChangedDomainEvent>(domainEvent => domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheJobHasChildJobs_ShouldExecuteThemAfterCompletingItsPayload()
    {
        // Arrange
        _mockStagingResultsRepository.GetFilesToHashCountAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(0));
        _mockStagingResultsRepository.GetFilesToHashPageAsync(_scanId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<HashedFileSystemFileDto>>([]));

        IMediaLibraryScanJob mockChildJob = Substitute.For<IMediaLibraryScanJob>();
        mockChildJob.ExecuteAsync(Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _sut.AddChild(mockChildJob);
        Guid id = Guid.NewGuid();
        object input = new();

        // Act
        await _sut.ExecuteAsync(id, input, CancellationToken.None);

        // Assert
        await mockChildJob.Received(1).ExecuteAsync(id, input, Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }
}
