#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Contains unit tests for the <see cref="MusicFileSystemDiscoveryJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicFileSystemDiscoveryJobTests
{
    private const string ROOT_PATH = @"C:\Music";
    private const string AUDIO_FILE_PATH = @"C:\Music\song.mp3";
    private const string IMAGE_FILE_PATH = @"C:\Music\cover.jpg";

    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ILibraryScanStagingResultsRepository _mockStagingResultsRepository;
    private readonly IDirectoryScanFingerprintRepository _mockFingerprintRepository;
    private readonly IFileProviderService _mockFileProviderService;
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly MusicFileSystemDiscoveryJob _sut;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly DirectoryScanFingerprintEntityFixture _fingerprintFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryId _libraryId;
    private readonly ScanId _scanId;
    private readonly UserId _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicFileSystemDiscoveryJobTests"/> class.
    /// </summary>
    public MusicFileSystemDiscoveryJobTests()
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
        _mockFingerprintRepository = Substitute.For<IDirectoryScanFingerprintRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockStagingResultsRepository);
        _mockUnitOfWork.DirectoryScanFingerprintRepository.Returns(_mockFingerprintRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockFileProviderService = Substitute.For<IFileProviderService>();
        _mockDirectoryProviderService = Substitute.For<IDirectoryProviderService>();
        _mockServiceProvider.GetService(typeof(IFileProviderService)).Returns(_mockFileProviderService);
        _mockServiceProvider.GetService(typeof(IDirectoryProviderService)).Returns(_mockDirectoryProviderService);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        // Default stubs that keep the happy path working, overridden per test where needed.
        _mockStagingResultsRepository.InsertRangeAsync(Arg.Any<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(), Arg.Any<CancellationToken>()).Returns(Result.Created);
        _mockFingerprintRepository.UpsertRangeAsync(Arg.Any<IReadOnlyCollection<DirectoryScanFingerprintEntity>>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockFileProviderService.GetSize(Arg.Any<FileSystemPathId>()).Returns(Result.From<long?>(0));
        _mockFileProviderService.GetLastWriteTime(Arg.Any<FileSystemPathId>()).Returns(Result.From(Optional<DateTime>.None()));
        _mockDirectoryProviderService.GetLastWriteTime(Arg.Any<FileSystemPathId>()).Returns(Result.From(Optional<DateTime>.None()));

        _libraryId = _libraryIdFixture.Create();
        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _sut = new MusicFileSystemDiscoveryJob(_mockServiceScopeFactory)
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
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
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
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent => domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
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
    public async Task ExecuteAsync_WhenTheLibraryHasAudioFiles_ShouldStageOnlyTheAudioFilesAndComplete()
    {
        // Arrange
        SetupLibrary();
        SetupDirectory(ROOT_PATH, files: [AUDIO_FILE_PATH, IMAGE_FILE_PATH], subdirectories: []);
        _mockFileProviderService.GetSize(Arg.Is<FileSystemPathId>(path => path.Path == AUDIO_FILE_PATH)).Returns(Result.From<long?>(1234L));
        _mockFileProviderService.GetLastWriteTime(Arg.Is<FileSystemPathId>(path => path.Path == AUDIO_FILE_PATH))
            .Returns(Result.From(Optional<DateTime>.Some(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockStagingResultsRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(entities => entities.Count == 1
                && entities.First().Path == AUDIO_FILE_PATH
                && entities.First().Size == 1234
                && entities.First().LibraryScanId == _scanId.Value
                && entities.First().NeedsRehash),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheSubdirectoryHasAudioFiles_ShouldTraverseIntoIt()
    {
        // Arrange
        const string SUBDIRECTORY_PATH = @"C:\Music\Album";
        SetupLibrary();
        SetupDirectory(ROOT_PATH, files: [], subdirectories: [SUBDIRECTORY_PATH]);
        SetupDirectory(SUBDIRECTORY_PATH, files: [AUDIO_FILE_PATH], subdirectories: []);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockStagingResultsRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(entities => entities.Count == 1 && entities.First().Path == AUDIO_FILE_PATH),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFastSkipIsEnabledAndTheDirectoryIsUnchanged_ShouldSkipIt()
    {
        // Arrange
        DateTime lastWriteTime = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SetupLibrary(shouldSkipUnchangedDirectoriesDuringScan: true);
        _mockDirectoryProviderService.GetLastWriteTime(Arg.Is<FileSystemPathId>(path => path.Path == ROOT_PATH))
            .Returns(Result.From(Optional<DateTime>.Some(lastWriteTime)));
        _mockFingerprintRepository.GetMappedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new Dictionary<string, DirectoryScanFingerprintEntity>
            {
                [ROOT_PATH] = _fingerprintFixture.Create(libraryId: _libraryId.Value, path: ROOT_PATH, lastWriteTimeUtc: lastWriteTime)
            }));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        _mockFileProviderService.DidNotReceive().GetFilePaths(Arg.Any<FileSystemPathId>(), true);
        await _mockStagingResultsRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(), Arg.Any<CancellationToken>());
        await _mockFingerprintRepository.DidNotReceive().UpsertRangeAsync(Arg.Any<IReadOnlyCollection<DirectoryScanFingerprintEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFastSkipIsEnabledAndTheDirectoryChanged_ShouldStoreAFingerprint()
    {
        // Arrange
        DateTime lastWriteTime = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SetupLibrary(shouldSkipUnchangedDirectoriesDuringScan: true);
        _mockDirectoryProviderService.GetLastWriteTime(Arg.Is<FileSystemPathId>(path => path.Path == ROOT_PATH))
            .Returns(Result.From(Optional<DateTime>.Some(lastWriteTime)));
        _mockFingerprintRepository.GetMappedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new Dictionary<string, DirectoryScanFingerprintEntity>
            {
                [ROOT_PATH] = _fingerprintFixture.Create(libraryId: _libraryId.Value, path: ROOT_PATH, lastWriteTimeUtc: lastWriteTime.AddDays(-1))
            }));
        SetupDirectory(ROOT_PATH, files: [], subdirectories: []);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockFingerprintRepository.Received(1).UpsertRangeAsync(
            Arg.Is<IReadOnlyCollection<DirectoryScanFingerprintEntity>>(fingerprints => fingerprints.Count == 1 && fingerprints.First().Path == ROOT_PATH),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFileSizeAndLastWriteTimeCannotBeRead_ShouldStageTheFileWithDefaults()
    {
        // Arrange
        SetupLibrary();
        SetupDirectory(ROOT_PATH, files: [AUDIO_FILE_PATH], subdirectories: []);
        _mockFileProviderService.GetSize(Arg.Any<FileSystemPathId>()).Returns(Error.Failure("FileSystem.Error", "Failed to read the size"));
        _mockFileProviderService.GetLastWriteTime(Arg.Any<FileSystemPathId>()).Returns(Error.Failure("FileSystem.Error", "Failed to read the last write time"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        await _mockStagingResultsRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(entities => entities.Count == 1 && entities.First().Size == 0 && entities.First().Ticks == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenReadingTheDirectoryFails_ShouldSkipItAndComplete()
    {
        // Arrange
        SetupLibrary();
        _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true).Returns(Error.Failure("FileSystem.Error", "Failed to read the files"));
        _mockDirectoryProviderService.GetSubdirectoryPaths(Arg.Any<FileSystemPathId>(), true).Returns(Error.Failure("FileSystem.Error", "Failed to read the subdirectories"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockStagingResultsRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheStagingInsertFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        SetupLibrary();
        SetupDirectory(ROOT_PATH, files: [AUDIO_FILE_PATH], subdirectories: []);
        _mockStagingResultsRepository.InsertRangeAsync(Arg.Any<IReadOnlyCollection<LibraryScanStagingResultsEntity>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to stage the discovered files"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent => domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFingerprintUpsertFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        SetupLibrary(shouldSkipUnchangedDirectoriesDuringScan: true);
        SetupDirectory(ROOT_PATH, files: [], subdirectories: []);
        _mockFingerprintRepository.GetMappedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new Dictionary<string, DirectoryScanFingerprintEntity>()));
        _mockFingerprintRepository.UpsertRangeAsync(Arg.Any<IReadOnlyCollection<DirectoryScanFingerprintEntity>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to store the fingerprints"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheJobIsCompleted_ShouldPublishProgressAndCallTheChildren()
    {
        // Arrange
        SetupLibrary();
        SetupDirectory(ROOT_PATH, files: [], subdirectories: []);
        IMediaLibraryScanJob mockChild = Substitute.For<IMediaLibraryScanJob>();
        mockChild.ExecuteAsync(Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sut.AddChild(mockChild);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockDomainEventPublisher.Received().PublishAsync(Arg.Is<LibraryScanProgressChangedDomainEvent>(domainEvent => domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
        await mockChild.Received(1).ExecuteAsync(Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Stubs the media library reader with a library whose single content location is the test root path.
    /// </summary>
    /// <param name="shouldSkipUnchangedDirectoriesDuringScan">Whether the library skips the directories that have not changed since the last scan.</param>
    private void SetupLibrary(bool shouldSkipUnchangedDirectoriesDuringScan = false)
    {
        LibraryEntity library = _libraryEntityFixture.Create(
            id: _libraryId.Value,
            libraryType: LibraryType.Music,
            contentLocations: [ROOT_PATH],
            shouldSkipUnchangedDirectoriesDuringScan: shouldSkipUnchangedDirectoriesDuringScan);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }

    /// <summary>
    /// Stubs the file and directory providers for the provided directory.
    /// </summary>
    /// <param name="directory">The file system path of the directory.</param>
    /// <param name="files">The file system paths of the files of the directory.</param>
    /// <param name="subdirectories">The file system paths of the subdirectories of the directory.</param>
    private void SetupDirectory(string directory, string[] files, string[] subdirectories)
    {
        _mockFileProviderService.GetFilePaths(Arg.Is<FileSystemPathId>(path => path.Path == directory), true)
            .Returns(Result.From<IEnumerable<FileSystemPathId>>([.. files.Select(file => PathId(file))]));
        _mockDirectoryProviderService.GetSubdirectoryPaths(Arg.Is<FileSystemPathId>(path => path.Path == directory), true)
            .Returns(Result.From<IEnumerable<FileSystemPathId>>([.. subdirectories.Select(subdirectoryPath => PathId(subdirectoryPath))]));
    }

    /// <summary>
    /// Creates a file system path id for the provided path.
    /// </summary>
    /// <param name="path">The path to create the id for.</param>
    /// <returns>The created file system path id.</returns>
    private static FileSystemPathId PathId(string path)
    {
        return FileSystemPathId.Create(path).Value;
    }
}
