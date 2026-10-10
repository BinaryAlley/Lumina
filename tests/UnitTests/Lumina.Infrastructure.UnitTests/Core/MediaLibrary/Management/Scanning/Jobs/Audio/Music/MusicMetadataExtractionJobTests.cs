#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
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
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
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
/// Contains unit tests for the <see cref="MusicMetadataExtractionJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMetadataExtractionJobTests
{
    private const string ROOT_PATH = @"C:\Music";
    private const string AUDIO_FILE_PATH = @"C:\Music\Queen\Bohemian Rhapsody.mp3";

    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMusicLibraryScanItemMetadataRepository _mockMusicMetadataRepository;
    private readonly ILibraryScanStagingResultsRepository _mockStagingResultsRepository;
    private readonly ILibraryPathTemplateService _mockPathTemplateService;
    private readonly IPathService _mockPathService;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly MusicMetadataExtractionJob _sut;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryPathTemplateFixture _libraryPathTemplateFixture = new();
    private readonly ParsedLibraryPathFixture _parsedLibraryPathFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryId _libraryId;
    private readonly ScanId _scanId;
    private readonly UserId _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMetadataExtractionJobTests"/> class.
    /// </summary>
    public MusicMetadataExtractionJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMusicMetadataRepository = Substitute.For<IMusicLibraryScanItemMetadataRepository>();
        _mockStagingResultsRepository = Substitute.For<ILibraryScanStagingResultsRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.MusicLibraryScanItemMetadataRepository.Returns(_mockMusicMetadataRepository);
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockStagingResultsRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockPathTemplateService = Substitute.For<ILibraryPathTemplateService>();
        _mockPathService = Substitute.For<IPathService>();
        _mockPathService.PathSeparator.Returns('\\');
        _mockServiceProvider.GetService(typeof(ILibraryPathTemplateService)).Returns(_mockPathTemplateService);
        _mockServiceProvider.GetService(typeof(IPathService)).Returns(_mockPathService);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        // Default stubs that keep the happy path working, overridden per test where needed.
        _mockMusicMetadataRepository.InsertRangeAsync(Arg.Any<IReadOnlyCollection<MusicLibraryScanItemMetadataEntity>>(), Arg.Any<CancellationToken>()).Returns(Result.Created);
        _mockPathTemplateService.ResolveTemplate(Arg.Any<LibraryType>(), Arg.Any<IReadOnlyList<LibraryPathPart>>()).Returns(Result.From(_libraryPathTemplateFixture.Create()));
        _mockPathTemplateService.Parse(Arg.Any<LibraryType>(), Arg.Any<LibraryPathTemplate>(), Arg.Any<string>(), Arg.Any<char>()).Returns(Result.From<Optional<ParsedLibraryPath>>(Optional<ParsedLibraryPath>.None()));
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _libraryId = _libraryIdFixture.Create();
        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _sut = new MusicMetadataExtractionJob(_mockServiceScopeFactory, Substitute.For<Microsoft.Extensions.Logging.ILogger<MusicMetadataExtractionJob>>())
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
        await _mockMusicMetadataRepository.DidNotReceive().InsertRangeAsync(Arg.Any<IReadOnlyCollection<MusicLibraryScanItemMetadataEntity>>(), Arg.Any<CancellationToken>());
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
    public async Task ExecuteAsync_WhenThereArePathsToProcess_ShouldStageTheirMetadataAndComplete()
    {
        // Arrange
        SetupLibrary();
        SetupPaths(AUDIO_FILE_PATH);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockMusicMetadataRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicLibraryScanItemMetadataEntity>>(items => items.Count == 1
                && items.First().Path == AUDIO_FILE_PATH
                && items.First().LibraryScanId == _scanId.Value
                && items.First().LibraryId == _libraryId.Value
                && items.First().TrackTitle == "Bohemian Rhapsody"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThePathMatchesTheTemplate_ShouldApplyThePathDerivedMetadata()
    {
        // Arrange
        SetupLibrary();
        SetupPaths(AUDIO_FILE_PATH);
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Queen",
            [LibraryPathPartKind.ReleaseType] = "Album",
            [LibraryPathPartKind.ReleaseYear] = "1975",
            [LibraryPathPartKind.ReleaseName] = "A Night at the Opera",
            [LibraryPathPartKind.TrackNumber] = "11",
            [LibraryPathPartKind.DiscNumber] = "1",
            [LibraryPathPartKind.TrackName] = "Bohemian Rhapsody"
        });
        _mockPathTemplateService.Parse(Arg.Any<LibraryType>(), Arg.Any<LibraryPathTemplate>(), Arg.Any<string>(), Arg.Any<char>())
            .Returns(Result.From<Optional<ParsedLibraryPath>>(Optional<ParsedLibraryPath>.Some(parsedPath)));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        await _mockMusicMetadataRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicLibraryScanItemMetadataEntity>>(items => items.Count == 1
                && items.First().ArtistName == "Queen"
                && items.First().ReleaseType == MusicReleaseType.Album
                && items.First().ReleaseYear == 1975
                && items.First().ReleaseName == "A Night at the Opera"
                && items.First().TrackNumber == 11
                && items.First().DiscNumber == 1
                && items.First().TrackTitle == "Bohemian Rhapsody"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheStagingInsertFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        SetupLibrary();
        SetupPaths(AUDIO_FILE_PATH);
        _mockMusicMetadataRepository.InsertRangeAsync(Arg.Any<IReadOnlyCollection<MusicLibraryScanItemMetadataEntity>>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to stage the extracted metadata"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent => domainEvent.LibraryId == _libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheJobIsCompleted_ShouldPublishProgressAndCallTheChildren()
    {
        // Arrange
        SetupLibrary();
        SetupPaths(AUDIO_FILE_PATH);
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
    private void SetupLibrary()
    {
        LibraryEntity library = _libraryEntityFixture.Create(
            id: _libraryId.Value,
            libraryType: LibraryType.Music,
            contentLocations: [ROOT_PATH]);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }

    /// <summary>
    /// Stubs the staging results repository with the paths that need their metadata extracted.
    /// </summary>
    /// <param name="paths">The file system paths that need their metadata extracted.</param>
    private void SetupPaths(params string[] paths)
    {
        _mockStagingResultsRepository.GetPathsNeedingRehashAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>(paths));
        _mockPathService.GetFileNameWithoutExtension(Arg.Any<string>()).Returns("Bohemian Rhapsody");
        _mockPathService.GetFileName(Arg.Any<string>()).Returns("Bohemian Rhapsody.mp3");
    }
}
