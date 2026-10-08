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
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;

/// <summary>
/// Contains unit tests for the <see cref="MusicArtworkService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtworkServiceTests
{
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

        MediaSettingsDto mediaSettings = _mediaSettingsDtoFixture.Create(rootDirectory: "media", librariesDirectory: "libraries", booksDirectory: "books", musicDirectory: "music");
        _mockMediaSettingsOptions = Substitute.For<IOptions<MediaSettingsDto>>();
        _mockMediaSettingsOptions.Value.Returns(mediaSettings);

        _sut = new MusicArtworkService(_mockEnvironmentContext, _mockPathService, _mockHttpClientFactory, _mockMediaSettingsOptions);
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenLocalArtworkDoesNotExist_ShouldReturnFileNotFound()
    {
        // Arrange
        string sourcePath = Path.Combine(Path.GetTempPath(), $"missing-artwork-{Guid.NewGuid():N}.jpg");
        MockPathBuilding();
        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(false));

        // Act
        Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(localPath: sourcePath), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
        await _mockFileTypeService.DidNotReceive().GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenArtworkHasNeitherLocalPathNorRemoteUrl_ShouldReturnFileNotFound()
    {
        // Arrange
        MockPathBuilding();

        // Act
        Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public async Task SaveAlbumArtworkAsync_WhenFileExistenceCheckFails_ShouldReturnTheError()
    {
        // Arrange
        string sourcePath = Path.Combine(Path.GetTempPath(), $"missing-artwork-{Guid.NewGuid():N}.jpg");
        MockPathBuilding();
        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>())
            .Returns(Error.Failure("FileSystem.Error", "Failed to check the artwork file"));

        // Act
        Result<string> result = await _sut.SaveAlbumArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album", _artworkDtoFixture.Create(localPath: sourcePath), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("FileSystem.Error", result.FirstError.Code);
    }

    [Fact]
    public void DeleteAlbumArtwork_WhenListingTheArtworkFilesFails_ShouldReturnTheError()
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
        _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true)
            .Returns(Error.Failure("FileSystem.Error", "Failed to list the artwork files"));

        // Act
        Result<Deleted> result = _sut.DeleteAlbumArtwork(libraryId, albumId, libraryName, artistName, albumTitle);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("FileSystem.Error", result.FirstError.Code);
        _mockFileProviderService.DidNotReceive().DeleteFile(Arg.Any<FileSystemPathId>());
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
    public void DeleteAlbumArtwork_WhenCalledWithInvalidNameSegments_ShouldReturnError()
    {
        // Arrange
        _mockPathService.SanitizeSegment(Arg.Any<string>())
            .Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        Result<Deleted> result = _sut.DeleteAlbumArtwork(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album");

        // Assert
        Assert.True(result.IsFailure);
        _mockFileProviderService.DidNotReceive().GetFilePaths(Arg.Any<FileSystemPathId>(), true);
    }

    [Fact]
    public void DeleteAlbumArtwork_WhenBuildingTheDirectoryPathFails_ShouldReturnTheError()
    {
        // Arrange
        _mockPathService.CombinePath(AppContext.BaseDirectory, "media").Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        Result<Deleted> result = _sut.DeleteAlbumArtwork(Guid.NewGuid(), Guid.NewGuid(), "Library", "Artist", "Album");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
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
}
