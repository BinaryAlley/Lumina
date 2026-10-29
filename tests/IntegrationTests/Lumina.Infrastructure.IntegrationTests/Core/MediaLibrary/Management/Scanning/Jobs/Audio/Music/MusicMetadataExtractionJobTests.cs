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
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Contains integration tests for the <see cref="MusicMetadataExtractionJob"/> class, probing real audio containers through the tagging library.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMetadataExtractionJobTests
{
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
        _mockPathService.GetFileNameWithoutExtension(Arg.Any<string>()).Returns("tone");
        _mockPathService.GetFileName(Arg.Any<string>()).Returns("tone.wav");

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
    public async Task ExecuteAsync_WhenTheFileIsARealAudioFile_ShouldCaptureItsContainerAndAudioProperties()
    {
        // Arrange
        using TemporaryMediaDirectory temporaryMediaDirectory = new();
        string audioFilePath = temporaryMediaDirectory.CreateWavFile("tone.wav");
        LibraryEntity library = _libraryEntityFixture.Create(
            id: _libraryId.Value,
            libraryType: LibraryType.Music,
            contentLocations: [temporaryMediaDirectory.Path]);
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockStagingResultsRepository.GetPathsNeedingRehashAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([audioFilePath]));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockMusicMetadataRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicLibraryScanItemMetadataEntity>>(items => items.Count == 1
                && items.First().Container == "WAV"
                && items.First().SampleRate == 8000
                && items.First().Channels == 1
                && items.First().BitDepth == 16),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheContainerHasNoDedicatedParser_ShouldFallBackToTheMimeTypeName()
    {
        // Arrange
        using TemporaryMediaDirectory temporaryMediaDirectory = new();
        string imageFilePath = temporaryMediaDirectory.CreatePngFile("cover.png");
        LibraryEntity library = _libraryEntityFixture.Create(
            id: _libraryId.Value,
            libraryType: LibraryType.Music,
            contentLocations: [temporaryMediaDirectory.Path]);
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockStagingResultsRepository.GetPathsNeedingRehashAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([imageFilePath]));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockMusicMetadataRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicLibraryScanItemMetadataEntity>>(items => items.Count == 1
                && items.First().Container == "PNG"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFileIsAReparsePoint_ShouldNotReadTheLinkedFile()
    {
        // Arrange
        using TemporaryMediaDirectory temporaryMediaDirectory = new();
        using TemporaryMediaDirectory outsideMediaDirectory = new();
        string targetFilePath = outsideMediaDirectory.CreateWavFile("target.wav");
        string linkFilePath = System.IO.Path.Combine(temporaryMediaDirectory.Path, "link.wav");
        if (!TryCreateFileSymbolicLink(linkFilePath, targetFilePath))
            return; // The environment does not support creating file symbolic links.

        LibraryEntity library = _libraryEntityFixture.Create(
            id: _libraryId.Value,
            libraryType: LibraryType.Music,
            contentLocations: [temporaryMediaDirectory.Path]);
        _mockLibraryRepository.GetByIdAsync(_libraryId.Value, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockStagingResultsRepository.GetPathsNeedingRehashAsync(_scanId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<string>>([linkFilePath]));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // The linked file lives outside the content location, so its audio metadata must not be read: the container stays unrecognized.
        await _mockMusicMetadataRepository.Received(1).InsertRangeAsync(
            Arg.Is<IReadOnlyCollection<MusicLibraryScanItemMetadataEntity>>(items => items.Count == 1
                && items.First().Container == null
                && items.First().SampleRate == 0),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Creates a file system symbolic link that points to the target file.
    /// </summary>
    /// <param name="linkPath">The file system path of the symbolic link to create.</param>
    /// <param name="targetPath">The file system path of the file the link points to.</param>
    /// <returns><see langword="true"/> when the link was created, <see langword="false"/> otherwise.</returns>
    private static bool TryCreateFileSymbolicLink(string linkPath, string targetPath)
    {
        try
        {
            File.CreateSymbolicLink(linkPath, targetPath);
            return true;
        }
        catch (Exception)
        {
            // Creating a file symbolic link requires elevated privileges on Windows, so the environment may not support it.
            return false;
        }
    }

    /// <summary>
    /// Test helper managing a temporary directory holding real media files, deleted when the test finishes.
    /// </summary>
    private sealed class TemporaryMediaDirectory : IDisposable
    {
        /// <summary>
        /// A minimal, valid, one by one pixel PNG image, used to exercise a container that has no dedicated parser.
        /// </summary>
        private const string PNG_BYTES = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXY2BgYGAAAAAFAAGKM+MAAAAAAElFTkSuQmCC";

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryMediaDirectory"/> class.
        /// </summary>
        public TemporaryMediaDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"lumina-music-probe-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        /// <summary>
        /// Gets the full path of the temporary directory.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Writes a minimal, valid, one second long 16 bit PCM mono 8000 Hz WAV file into the temporary directory.
        /// </summary>
        /// <param name="fileName">The name of the WAV file to create.</param>
        /// <returns>The full path of the created WAV file.</returns>
        public string CreateWavFile(string fileName)
        {
            const int SAMPLE_RATE = 8000;
            const short CHANNELS = 1;
            const short BITS_PER_SAMPLE = 16;
            int blockAlign = CHANNELS * (BITS_PER_SAMPLE / 8);
            int dataSize = SAMPLE_RATE * blockAlign;
            string filePath = System.IO.Path.Combine(Path, fileName);

            using (FileStream stream = new(filePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new(stream))
            {
                writer.Write("RIFF"u8.ToArray());
                writer.Write(36 + dataSize);
                writer.Write("WAVE"u8.ToArray());
                writer.Write("fmt "u8.ToArray());
                writer.Write(16);
                writer.Write((short)1);
                writer.Write(CHANNELS);
                writer.Write(SAMPLE_RATE);
                writer.Write(SAMPLE_RATE * blockAlign);
                writer.Write((short)blockAlign);
                writer.Write(BITS_PER_SAMPLE);
                writer.Write("data"u8.ToArray());
                writer.Write(dataSize);
                writer.Write(new byte[dataSize]);
            }
            return filePath;
        }

        /// <summary>
        /// Writes a minimal, valid PNG image into the temporary directory.
        /// </summary>
        /// <param name="fileName">The name of the PNG file to create.</param>
        /// <returns>The full path of the created PNG file.</returns>
        public string CreatePngFile(string fileName)
        {
            string filePath = System.IO.Path.Combine(Path, fileName);
            File.WriteAllBytes(filePath, Convert.FromBase64String(PNG_BYTES));
            return filePath;
        }

        /// <summary>
        /// Deletes the temporary directory, ignoring the failures of the files that are still in use.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch (IOException)
            {
                // Best effort cleanup of the temporary directory.
            }
            _disposed = true;
        }
    }
}
