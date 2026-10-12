#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Infrastructure.Fixtures.Common.Setup;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;

/// <summary>
/// Contains integration tests for the <see cref="MusicArtworkService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtworkServiceTests
{
    private const long MAX_ARTWORK_SIZE_BYTES = 10 * 1024 * 1024;

    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IFileTypeService _mockFileTypeService;
    private readonly IFileProviderService _mockFileProviderService;
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IPathService _mockPathService;
    private readonly IHttpClientFactory _mockHttpClientFactory;
    private readonly IOptions<MediaSettingsDto> _mockMediaSettingsOptions;
    private readonly MusicArtworkService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly PathSegmentFixture _pathSegmentFixture = new();
    private readonly ArtworkDtoFixture _artworkDtoFixture = new();
    private readonly MediaSettingsDtoFixture _mediaSettingsDtoFixture = new();
    private readonly StubHttpMessageHandler _stubHttpMessageHandler = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtworkServiceTests"/> class.
    /// </summary>
    public MusicArtworkServiceTests()
    {
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockFileTypeService = Substitute.For<IFileTypeService>();
        _mockFileProviderService = Substitute.For<IFileProviderService>();
        _mockDirectoryProviderService = Substitute.For<IDirectoryProviderService>();
        _mockEnvironmentContext.FileTypeService.Returns(_mockFileTypeService);
        _mockEnvironmentContext.FileProviderService.Returns(_mockFileProviderService);
        _mockEnvironmentContext.DirectoryProviderService.Returns(_mockDirectoryProviderService);

        _mockPathService = Substitute.For<IPathService>();
        _mockPathService.PathSeparator.Returns('\\');

        _mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        _mockHttpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(_stubHttpMessageHandler));

        MediaSettingsDto mediaSettings = _mediaSettingsDtoFixture.Create(rootDirectory: "media", librariesDirectory: "libraries", booksDirectory: "books", musicDirectory: "music");
        _mockMediaSettingsOptions = Substitute.For<IOptions<MediaSettingsDto>>();
        _mockMediaSettingsOptions.Value.Returns(mediaSettings);

        _sut = new MusicArtworkService(_mockEnvironmentContext, _mockPathService, _mockHttpClientFactory, _mockMediaSettingsOptions);
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenLocalArtworkExists_ShouldCopyItAndReturnARelativePath()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);

            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));

            string renamedFilePath = Path.Combine(artworkDirectoryPath, "cover.jpg");
            FileSystemPathId renamedFileId = _fileSystemPathIdFixture.Create(renamedFilePath);
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpg")
                .Returns(Result.From(renamedFileId));

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            Assert.Equal(renamedFilePath[AppContext.BaseDirectory.Length..].Insert(0, "\\"), result.Value);
            _mockFileProviderService.Received(1).RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpg");
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Theory]
    [InlineData(ArtworkType.Cover, 0, "cover.jpg")] // the front cover uses the bare type name
    [InlineData(ArtworkType.Back, 0, "back.jpg")] // the back cover uses the bare type name
    [InlineData(ArtworkType.Medium, 0, "medium.jpg")] // a single medium uses the bare type name
    [InlineData(ArtworkType.Booklet, 0, "booklet.jpg")] // the first booklet page uses the bare type name
    [InlineData(ArtworkType.Booklet, 2, "booklet-2.jpg")] // a further booklet page is suffixed with its ordinal
    [InlineData(ArtworkType.Liner, 1, "liner-1.jpg")] // a liner note page is suffixed with its ordinal
    [InlineData(ArtworkType.Other, 0, "other.jpg")] // any other type falls back to the generic name
    public async Task SaveAlbumArtworkAsync_WhenTheArtworkHasAType_ShouldNameTheFileAfterTheTypeAndOrdinal(ArtworkType artworkType, int ordinal, string expectedFileName)
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);

            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "artwork.jpg"))));
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), expectedFileName)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, expectedFileName))));

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(localPath: sourcePath, type: artworkType, ordinal: ordinal), null, CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            _mockFileProviderService.Received(1).RenameFile(Arg.Any<FileSystemPathId>(), expectedFileName);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenArtistNameIsMissing_ShouldUseUnknownForTheDirectorySegment()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName: "Unknown", albumTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);

            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpg")
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName: "   ", albumTitle, _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            _mockPathService.Received(1).SanitizeSegment("Unknown");
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenLocalArtworkIsALink_ShouldReturnInvalidPath()
    {
        // Arrange
        string targetDirectory = Path.Combine(Path.GetTempPath(), $"lumina-junction-target-{Guid.NewGuid():N}");
        Directory.CreateDirectory(targetDirectory);
        string linkPath = Path.Combine(Path.GetTempPath(), $"lumina-junction-link-{Guid.NewGuid():N}");
        if (!TryCreateJunction(linkPath, targetDirectory))
        {
            Directory.Delete(targetDirectory, true);
            return; // The environment does not support creating reparse points.
        }

        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            MockPathBuilding();

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(localPath: linkPath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
            _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true);
        }
        finally
        {
            if (OperatingSystem.IsWindows())
                Directory.Delete(linkPath, true);
            else
                File.Delete(linkPath);
            Directory.Delete(targetDirectory, true);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenLocalArtworkExceedsMaxSize_ShouldReturnFileTooLarge()
    {
        // Arrange
        string sourcePath = CreateTempImageFile(smallImageBytes: (int)MAX_ARTWORK_SIZE_BYTES + 1);
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            MockPathBuilding();

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(Errors.FileSystemManagement.FileTooLarge, result.FirstError);
            await _mockFileTypeService.DidNotReceive().GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>());
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenLocalArtworkIsNotAnImage_ShouldReturnCoverFileMustBeAnImage()
    {
        // Arrange
        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.None));
            MockPathBuilding();

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(Errors.Library.CoverFileMustBeAnImage, result.FirstError);
            _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenRemoteUrlIsProvided_ShouldDownloadAndStoreTheArtwork()
    {
        // Arrange
        _stubHttpMessageHandler.SetResponse(HttpStatusCode.OK, new byte[1024]);
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
        _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.JPEG));

        string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
        MockArtworkDirectoryStubs(artworkDirectoryPath);

        _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));
        _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpg")
            .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));

        // Act
        Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(remoteUrl: "https://coverartarchive.org/cover.jpg"), null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockFileProviderService.Received(1).CopyFile(
            Arg.Is<FileSystemPathId>(pathId => pathId.Path.StartsWith(Path.GetTempPath())),
            Arg.Any<FileSystemPathId>(),
            true);
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenRemoteDownloadReturnsAnErrorStatus_ShouldReturnFileNotFound()
    {
        // Arrange
        _stubHttpMessageHandler.SetResponse(HttpStatusCode.NotFound, []);
        MockPathBuilding();

        // Act
        Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(remoteUrl: "https://coverartarchive.org/cover.jpg"), null, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenRemoteDownloadExceedsMaxSize_ShouldReturnFileTooLarge()
    {
        // Arrange
        _stubHttpMessageHandler.SetResponse(HttpStatusCode.OK, new byte[MAX_ARTWORK_SIZE_BYTES + 1]);
        MockPathBuilding();

        // Act
        Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(remoteUrl: "https://coverartarchive.org/cover.jpg"), null, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileTooLarge, result.FirstError);
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenArtworkHasBothLocalPathAndRemoteUrl_ShouldUseTheLocalFileWithoutDeletingIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);

            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpg")
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(localPath: sourcePath, remoteUrl: "https://coverartarchive.org/cover.jpg"), null, CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            Assert.True(File.Exists(sourcePath));
            _mockFileProviderService.Received(1).CopyFile(
                Arg.Is<FileSystemPathId>(pathId => pathId.Path == sourcePath),
                Arg.Any<FileSystemPathId>(),
                true);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenRemoteDownloadIsCancelled_ShouldRethrowCancellation()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        MockPathBuilding();

        // Act
        async Task Act()
        {
            await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(remoteUrl: "https://coverartarchive.org/cover.jpg"), null, cancellationTokenSource.Token);
        }

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenImageTypeDetectionFails_ShouldReturnTheError()
    {
        // Arrange
        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Error.Failure("FileSystem.Error", "Failed to detect the image type"));
            MockPathBuilding();

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("FileSystem.Error", result.FirstError.Code);
            _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenCreatingTheArtworkDirectoryFails_ShouldReturnTheError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);
            _mockDirectoryProviderService.CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
                .Returns(Error.Failure("Directory.Error", "Failed to create the artwork directory"));

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Directory.Error", result.FirstError.Code);
            _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenDirectoryExistenceCheckFails_ShouldReturnTheError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);
            _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>())
                .Returns(Error.Failure("Directory.Error", "Failed to check the artwork directory"));

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Directory.Error", result.FirstError.Code);
            _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenCopyingTheArtworkFails_ShouldReturnTheError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);
            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Error.Failure("FileSystem.Error", "Failed to copy the artwork"));

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("FileSystem.Error", result.FirstError.Code);
            _mockFileProviderService.DidNotReceive().RenameFile(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenRenamingTheCopiedArtworkFails_ShouldReturnTheError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);
            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpg")
                .Returns(Error.Failure("FileSystem.Error", "Failed to rename the artwork"));

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("FileSystem.Error", result.FirstError.Code);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenArtistNameCannotBeSanitized_ShouldReturnInvalidPath()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string sourcePath = CreateTempImageFile();
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string mediaRoot = Path.Combine(AppContext.BaseDirectory, "media");
            string musicPath = Path.Combine(mediaRoot, "music");
            _mockPathService.CombinePath(AppContext.BaseDirectory, "media").Returns(Result.From(mediaRoot));
            _mockPathService.CombinePath(mediaRoot, "music").Returns(Result.From(musicPath));
            _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
                .Returns(callInfo => Result.From(Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1))));
            _mockPathService.SanitizeSegment(Arg.Any<string>())
                .Returns(callInfo =>
                {
                    string name = callInfo.Arg<string>();
                    return name == $"{libraryName}-{libraryId}"
                        ? Result.From(_pathSegmentFixture.Create(name: name, isDirectory: true, isDrive: false))
                        : Errors.FileSystemManagement.InvalidPath;
                });

            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, _artworkDtoFixture.Create(localPath: sourcePath), null, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
            _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public void DeleteAlbumArtwork_WhenCalled_ShouldDeleteEveryFileInTheAlbumDirectory()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";

        string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
        MockArtworkDirectoryStubs(artworkDirectoryPath);
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
        FileSystemPathId coverFileId = _fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"));
        FileSystemPathId bookletFileId = _fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "booklet-1.jpg"));
        _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From<IEnumerable<FileSystemPathId>>([coverFileId, bookletFileId]));
        _mockFileProviderService.DeleteFile(Arg.Any<FileSystemPathId>()).Returns(Result.Deleted);

        // Act
        Result<Deleted> result = _sut.DeleteAlbumArtwork(libraryId, albumId, libraryName, artistName, albumTitle);

        // Assert
        Assert.False(result.IsFailure);
        _mockFileProviderService.Received(1).DeleteFile(coverFileId);
        _mockFileProviderService.Received(1).DeleteFile(bookletFileId);
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenAnArtworkStreamIsUploaded_ShouldStoreTheFileOnDiskAndDeleteTheTemporaryUpload()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        string albumTitle = "Test Album";
        byte[] uploadedBytes = [10, 20, 30, 40, 50];
        string libraryDirectoryPath = BuildLibraryDirectoryPath(libraryName, libraryId);
        string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, albumId, libraryName, artistName, albumTitle);
        string expectedStoredPath = Path.Combine(artworkDirectoryPath, "cover.jpg");
        string? temporaryUploadPath = null;
        MockRealFileSystemStubs(pathId => temporaryUploadPath = pathId.Path);
        _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.JPEG));
        await using MemoryStream artworkStream = new(uploadedBytes);
        try
        {
            // Act
            Result<string> result = await _sut.SaveAlbumArtworkAsync(libraryId, albumId, libraryName, artistName, albumTitle, artworkStream, "cover.jpg", null, CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            Assert.Equal(expectedStoredPath[AppContext.BaseDirectory.Length..].Insert(0, "\\"), result.Value);
            Assert.True(File.Exists(expectedStoredPath));
            Assert.Equal(uploadedBytes, File.ReadAllBytes(expectedStoredPath));
            Assert.NotNull(temporaryUploadPath);
            Assert.StartsWith(Path.GetTempPath(), temporaryUploadPath);
            Assert.EndsWith(".jpg", temporaryUploadPath);
            Assert.False(File.Exists(temporaryUploadPath));
        }
        finally
        {
            if (Directory.Exists(libraryDirectoryPath))
                Directory.Delete(libraryDirectoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task SaveArtistArtworkAsync_WhenAnArtworkStreamIsUploaded_ShouldStoreTheFileOnDiskAndDeleteTheTemporaryUpload()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        string libraryName = "Test Library";
        string artistName = "Test Artist";
        byte[] uploadedBytes = [60, 70, 80, 90];
        string libraryDirectoryPath = BuildLibraryDirectoryPath(libraryName, libraryId);
        string artworkDirectoryPath = BuildArtistArtworkDirectoryPath(libraryId, libraryName, artistName);
        string expectedStoredPath = Path.Combine(artworkDirectoryPath, "cover.png");
        string? temporaryUploadPath = null;
        MockRealFileSystemStubs(pathId => temporaryUploadPath = pathId.Path);
        _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.PNG));
        await using MemoryStream artworkStream = new(uploadedBytes);
        try
        {
            // Act
            Result<string> result = await _sut.SaveArtistArtworkAsync(libraryId, libraryName, artistName, artworkStream, "cover.png", CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            Assert.Equal(expectedStoredPath[AppContext.BaseDirectory.Length..].Insert(0, "\\"), result.Value);
            Assert.True(File.Exists(expectedStoredPath));
            Assert.Equal(uploadedBytes, File.ReadAllBytes(expectedStoredPath));
            Assert.NotNull(temporaryUploadPath);
            Assert.StartsWith(Path.GetTempPath(), temporaryUploadPath);
            Assert.EndsWith(".png", temporaryUploadPath);
            Assert.False(File.Exists(temporaryUploadPath));
        }
        finally
        {
            if (Directory.Exists(libraryDirectoryPath))
                Directory.Delete(libraryDirectoryPath, recursive: true);
        }
    }

    [Fact]
    public void DeleteStoredArtwork_WhenTheFileExists_ShouldDeleteTheRealFile()
    {
        // Arrange
        string libraryDirectoryPath = Path.Combine(AppContext.BaseDirectory, "media", "music", $"delete-{Guid.NewGuid():N}");
        Directory.CreateDirectory(libraryDirectoryPath);
        string artworkPath = Path.Combine(libraryDirectoryPath, "cover.jpg");
        File.WriteAllBytes(artworkPath, [1, 2, 3]);
        string artworkRelativePath = artworkPath[AppContext.BaseDirectory.Length..].Insert(0, "\\");
        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>())
            .Returns(callInfo => Result.From(File.Exists(callInfo.Arg<FileSystemPathId>().Path)));
        _mockFileProviderService.DeleteFile(Arg.Any<FileSystemPathId>())
            .Returns(callInfo =>
            {
                File.Delete(callInfo.Arg<FileSystemPathId>().Path);
                return Result.Deleted;
            });
        try
        {
            // Act
            Result<Deleted> result = _sut.DeleteStoredArtwork(artworkRelativePath);

            // Assert
            Assert.False(result.IsFailure);
            Assert.False(File.Exists(artworkPath));
        }
        finally
        {
            if (Directory.Exists(libraryDirectoryPath))
                Directory.Delete(libraryDirectoryPath, recursive: true);
        }
    }

    [Fact]
    public void DeleteStoredArtwork_WhenTheFileIsMissing_ShouldReturnDeleted()
    {
        // Arrange
        string artworkRelativePath = $"\\media\\music\\missing-{Guid.NewGuid():N}.jpg";
        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(false));

        // Act
        Result<Deleted> result = _sut.DeleteStoredArtwork(artworkRelativePath);

        // Assert
        Assert.False(result.IsFailure);
        _mockFileProviderService.DidNotReceive().DeleteFile(Arg.Any<FileSystemPathId>());
    }

    [Theory]
    [InlineData(null)] // a null path is a no-op
    [InlineData("")] // an empty path is a no-op
    [InlineData("   ")] // a whitespace path is a no-op
    public void DeleteStoredArtwork_WhenThePathIsMissing_ShouldReturnDeletedWithoutTouchingTheFileSystem(string? artworkRelativePath)
    {
        // Act
        Result<Deleted> result = _sut.DeleteStoredArtwork(artworkRelativePath!);

        // Assert
        Assert.False(result.IsFailure);
        _mockFileProviderService.DidNotReceive().FileExists(Arg.Any<FileSystemPathId>());
        _mockFileProviderService.DidNotReceive().DeleteFile(Arg.Any<FileSystemPathId>());
    }

    /// <summary>
    /// Stubs the path and file system services so that the artwork storage pipeline performs real file system operations.
    /// </summary>
    /// <param name="onCopySource">Optional callback invoked with the source file of every file copy, used to observe the temporary upload.</param>
    private void MockRealFileSystemStubs(Action<FileSystemPathId>? onCopySource = null)
    {
        MockPathBuilding();

        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>())
            .Returns(callInfo => Result.From(File.Exists(callInfo.Arg<FileSystemPathId>().Path)));
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>())
            .Returns(callInfo => Result.From(Directory.Exists(callInfo.Arg<FileSystemPathId>().Path)));
        _mockDirectoryProviderService.CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(callInfo =>
            {
                string createdDirectoryPath = Path.Combine(callInfo.ArgAt<FileSystemPathId>(0).Path, callInfo.ArgAt<string>(1));
                Directory.CreateDirectory(createdDirectoryPath);
                return Result.From(_fileSystemPathIdFixture.Create(createdDirectoryPath));
            });
        _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
            .Returns(callInfo =>
            {
                FileSystemPathId sourcePathId = callInfo.ArgAt<FileSystemPathId>(0);
                FileSystemPathId destinationDirectoryPathId = callInfo.ArgAt<FileSystemPathId>(1);
                onCopySource?.Invoke(sourcePathId);
                string destinationFilePath = Path.Combine(destinationDirectoryPathId.Path, Path.GetFileName(sourcePathId.Path));
                File.Copy(sourcePathId.Path, destinationFilePath, overwrite: true);
                return Result.From(_fileSystemPathIdFixture.Create(destinationFilePath));
            });
        _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(callInfo =>
            {
                FileSystemPathId sourcePathId = callInfo.ArgAt<FileSystemPathId>(0);
                string newFileName = callInfo.ArgAt<string>(1);
                string renamedFilePath = Path.Combine(Path.GetDirectoryName(sourcePathId.Path)!, newFileName);
                File.Move(sourcePathId.Path, renamedFilePath);
                return Result.From(_fileSystemPathIdFixture.Create(renamedFilePath));
            });
        _mockFileProviderService.DeleteFile(Arg.Any<FileSystemPathId>())
            .Returns(callInfo =>
            {
                File.Delete(callInfo.Arg<FileSystemPathId>().Path);
                return Result.Deleted;
            });
    }

    /// <summary>
    /// Stubs the path service so that building the artwork directory paths succeeds.
    /// </summary>
    private void MockPathBuilding()
    {
        _mockPathService.SanitizeSegment(Arg.Any<string>())
            .Returns(callInfo =>
            {
                string name = callInfo.Arg<string>();
                return Result.From(_pathSegmentFixture.Create(name: name, isDirectory: true, isDrive: false));
            });

        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1))));
    }

    /// <summary>
    /// Stubs the path and file system services so that the artwork is stored into the given directory.
    /// </summary>
    /// <param name="artworkDirectoryPath">The file system path of the directory into which the artwork is stored.</param>
    private void MockArtworkDirectoryStubs(string artworkDirectoryPath)
    {
        MockPathBuilding();

        // Every directory except the artwork directory itself already exists, so that only the final directory is created.
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>())
            .Returns(callInfo => Result.From(!string.Equals(callInfo.Arg<FileSystemPathId>().Path, artworkDirectoryPath, StringComparison.OrdinalIgnoreCase)));
        _mockDirectoryProviderService.CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(_fileSystemPathIdFixture.Create(Path.Combine(callInfo.Arg<FileSystemPathId>().Path, callInfo.Arg<string>()))));

        _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From<IEnumerable<FileSystemPathId>>([]));
    }

    /// <summary>
    /// Builds the file system path of the directory that the artwork of the album is stored into.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <param name="libraryName">The name of the media library the album belongs to.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <param name="albumTitle">The title of the album.</param>
    /// <returns>The expected file system path of the album artwork directory.</returns>
    private static string BuildArtworkDirectoryPath(Guid libraryId, Guid albumId, string libraryName, string artistName, string albumTitle)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "media",
            "music",
            $"{libraryName}-{libraryId}",
            artistName,
            $"{albumTitle}-{albumId}");
    }

    /// <summary>
    /// Builds the file system path of the directory of a music media library.
    /// </summary>
    /// <param name="libraryName">The name of the media library.</param>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <returns>The expected file system path of the music media library directory.</returns>
    private static string BuildLibraryDirectoryPath(string libraryName, Guid libraryId)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "media",
            "music",
            $"{libraryName}-{libraryId}");
    }

    /// <summary>
    /// Builds the file system path of the directory that the artwork of the artist is stored into.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="libraryName">The name of the media library the artist belongs to.</param>
    /// <param name="artistName">The name of the artist.</param>
    /// <returns>The expected file system path of the artist artwork directory.</returns>
    private static string BuildArtistArtworkDirectoryPath(Guid libraryId, string libraryName, string artistName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "media",
            "music",
            $"{libraryName}-{libraryId}",
            artistName);
    }

    /// <summary>
    /// Creates a temporary image file and returns its path.
    /// </summary>
    /// <param name="smallImageBytes">The size of the temporary image file, in bytes.</param>
    /// <returns>The file system path of the created temporary image file.</returns>
    private static string CreateTempImageFile(int smallImageBytes = 1024)
    {
        string sourcePath = Path.Combine(Path.GetTempPath(), $"lumina-artwork-source-{Guid.NewGuid():N}.jpg");
        File.WriteAllBytes(sourcePath, new byte[smallImageBytes]);
        return sourcePath;
    }

    /// <summary>
    /// Creates a file system link that points to the target directory: a junction on Windows, and a symbolic link on Unix-like platforms.
    /// </summary>
    /// <param name="linkPath">The file system path of the link to create.</param>
    /// <param name="targetDirectory">The file system path of the directory the link points to.</param>
    /// <returns><see langword="true"/> when the link was created, <see langword="false"/> otherwise.</returns>
    private static bool TryCreateJunction(string linkPath, string targetDirectory)
    {
        try
        {
            ProcessStartInfo startInfo = OperatingSystem.IsWindows()
                ? new()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c mklink /J \"{linkPath}\" \"{targetDirectory}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
                : new()
                {
                    FileName = "ln",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
            if (!OperatingSystem.IsWindows())
            {
                startInfo.ArgumentList.Add("-s");
                startInfo.ArgumentList.Add(targetDirectory);
                startInfo.ArgumentList.Add(linkPath);
            }

            Process process = Process.Start(startInfo)!;
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
