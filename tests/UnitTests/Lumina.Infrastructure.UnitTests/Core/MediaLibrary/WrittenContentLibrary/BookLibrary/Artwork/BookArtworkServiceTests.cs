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
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Infrastructure.Fixtures.Common.Setup;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;

/// <summary>
/// Contains unit tests for the <see cref="BookArtworkService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookArtworkServiceTests
{
    private const long MAX_ARTWORK_SIZE_BYTES = 10 * 1024 * 1024;

    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IFileTypeService _mockFileTypeService;
    private readonly IFileProviderService _mockFileProviderService;
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IPathService _mockPathService;
    private readonly IHttpClientFactory _mockHttpClientFactory;
    private readonly IOptions<MediaSettingsDto> _mockMediaSettingsOptions;
    private readonly BookArtworkService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly PathSegmentFixture _pathSegmentFixture = new();
    private readonly ArtworkDtoFixture _artworkDtoFixture = new();
    private readonly MediaSettingsDtoFixture _mediaSettingsDtoFixture = new();
    private readonly StubHttpMessageHandler _stubHttpMessageHandler = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookArtworkServiceTests"/> class.
    /// </summary>
    public BookArtworkServiceTests()
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
        _mockHttpClientFactory.CreateClient().Returns(new HttpClient(_stubHttpMessageHandler));

        MediaSettingsDto mediaSettings = _mediaSettingsDtoFixture.Create(rootDirectory: "media", librariesDirectory: "libraries", booksDirectory: "books");
        _mockMediaSettingsOptions = Substitute.For<IOptions<MediaSettingsDto>>();
        _mockMediaSettingsOptions.Value.Returns(mediaSettings);

        _sut = new BookArtworkService(_mockEnvironmentContext, _mockPathService, _mockHttpClientFactory, _mockMediaSettingsOptions);
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenLocalArtworkExists_ShouldCopyItAndReturnARelativePath()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        string libraryName = "Test Library";
        string authorName = "Test Author";
        string bookTitle = "Test Book";

        string sourcePath = CreateTempImageFile(smallImageBytes: 1024);
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, bookId, libraryName, authorName, bookTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);

            FileSystemPathId copiedFileId = _fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"));
            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From(copiedFileId));

            string renamedFilePath = Path.Combine(artworkDirectoryPath, "cover.jpeg");
            FileSystemPathId renamedFileId = _fileSystemPathIdFixture.Create(renamedFilePath);
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpeg")
                .Returns(Result.From(renamedFileId));

            // Act
            Result<string> result = await _sut.SaveBookArtworkAsync(libraryId, bookId, libraryName, authorName, bookTitle, _artworkDtoFixture.Create(localPath: sourcePath), CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            Assert.Equal(renamedFilePath[AppContext.BaseDirectory.Length..].Insert(0, "\\"), result.Value);
            _mockFileProviderService.Received(1).CopyFile(
                Arg.Is<FileSystemPathId>(pathId => pathId.Path == sourcePath),
                Arg.Any<FileSystemPathId>(),
                true);
            _mockFileProviderService.Received(1).RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpeg");
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenAuthorNameIsMissing_ShouldUseUnknownForTheDirectorySegment()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        string libraryName = "Test Library";
        string bookTitle = "Test Book";

        string sourcePath = CreateTempImageFile(smallImageBytes: 1024);
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, bookId, libraryName, authorName: "Unknown", bookTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);

            FileSystemPathId renamedFileId = _fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpeg"));
            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpeg")
                .Returns(Result.From(renamedFileId));

            // Act
            Result<string> result = await _sut.SaveBookArtworkAsync(libraryId, bookId, libraryName, authorName: "   ", bookTitle, _artworkDtoFixture.Create(localPath: sourcePath), CancellationToken.None);

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
    public async Task SaveBookArtworkAsync_WhenLocalArtworkDoesNotExist_ShouldReturnFileNotFound()
    {
        // Arrange
        string sourcePath = Path.Combine(Path.GetTempPath(), $"missing-artwork-{Guid.NewGuid():N}.jpg");
        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(false));

        // Act
        Result<string> result = await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title", _artworkDtoFixture.Create(localPath: sourcePath), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
        await _mockFileTypeService.DidNotReceive().GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenLocalArtworkIsALink_ShouldReturnInvalidPath()
    {
        // Arrange
        string targetDirectory = Path.Combine(Path.GetTempPath(), $"lumina-junction-target-{Guid.NewGuid():N}");
        Directory.CreateDirectory(targetDirectory);
        string linkPath = Path.Combine(Path.GetTempPath(), $"lumina-junction-link-{Guid.NewGuid():N}");
        if (!TryCreateJunction(linkPath, targetDirectory))
        {
            Directory.Delete(targetDirectory, true);
            return; // the environment does not support creating reparse points
        }

        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));

            // Act
            Result<string> result = await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title", _artworkDtoFixture.Create(localPath: linkPath), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
            _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true);
        }
        finally
        {
            // Windows removes a junction with Directory.Delete, while Unix-like platforms unlink the symbolic link with File.Delete
            if (OperatingSystem.IsWindows())
                Directory.Delete(linkPath, true);
            else
                File.Delete(linkPath);
            Directory.Delete(targetDirectory, true);
        }
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenLocalArtworkExceedsMaxSize_ShouldReturnFileTooLarge()
    {
        // Arrange
        string sourcePath = CreateTempImageFile(smallImageBytes: (int)MAX_ARTWORK_SIZE_BYTES + 1);
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));

            // Act
            Result<string> result = await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title", _artworkDtoFixture.Create(localPath: sourcePath), CancellationToken.None);

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
    public async Task SaveBookArtworkAsync_WhenLocalArtworkIsNotAnImage_ShouldReturnCoverFileMustBeAnImage()
    {
        // Arrange
        string sourcePath = CreateTempImageFile(smallImageBytes: 1024);
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.None));

            // Act
            Result<string> result = await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title", _artworkDtoFixture.Create(localPath: sourcePath), CancellationToken.None);

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
    public async Task SaveBookArtworkAsync_WhenArtworkHasNeitherLocalPathNorRemoteUrl_ShouldReturnFileNotFound()
    {
        // Act
        Result<string> result = await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title", _artworkDtoFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenRemoteUrlIsProvided_ShouldDownloadAndStoreTheArtwork()
    {
        // Arrange
        byte[] imageBytes = new byte[1024];
        _stubHttpMessageHandler.SetResponse(HttpStatusCode.OK, imageBytes);
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        string libraryName = "Test Library";
        string authorName = "Test Author";
        string bookTitle = "Test Book";

        _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
        _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.JPEG));

        string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, bookId, libraryName, authorName, bookTitle);
        MockArtworkDirectoryStubs(artworkDirectoryPath);

        _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));
        FileSystemPathId renamedFileId = _fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpeg"));
        _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpeg")
            .Returns(Result.From(renamedFileId));

        // Act
        Result<string> result = await _sut.SaveBookArtworkAsync(libraryId, bookId, libraryName, authorName, bookTitle, _artworkDtoFixture.Create(remoteUrl: "http://artwork.example/cover.jpg"), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockFileProviderService.Received(1).CopyFile(
            Arg.Is<FileSystemPathId>(pathId => pathId.Path.StartsWith(Path.GetTempPath())),
            Arg.Any<FileSystemPathId>(),
            true);
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenRemoteDownloadReturnsAnErrorStatus_ShouldReturnFileNotFound()
    {
        // Arrange
        _stubHttpMessageHandler.SetResponse(HttpStatusCode.NotFound, []);

        // Act
        Result<string> result = await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title", _artworkDtoFixture.Create(remoteUrl: "http://artwork.example/cover.jpg"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenRemoteDownloadExceedsMaxSize_ShouldReturnFileTooLarge()
    {
        // Arrange
        _stubHttpMessageHandler.SetResponse(HttpStatusCode.OK, new byte[MAX_ARTWORK_SIZE_BYTES + 1]);

        // Act
        Result<string> result = await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title", _artworkDtoFixture.Create(remoteUrl: "http://artwork.example/cover.jpg"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileTooLarge, result.FirstError);
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenArtworkHasBothLocalPathAndRemoteUrl_ShouldUseTheLocalFileWithoutDeletingIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        string libraryName = "Test Library";
        string authorName = "Test Author";
        string bookTitle = "Test Book";

        string sourcePath = CreateTempImageFile(smallImageBytes: 1024);
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, bookId, libraryName, authorName, bookTitle);
            MockArtworkDirectoryStubs(artworkDirectoryPath);

            FileSystemPathId renamedFileId = _fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpeg"));
            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpg"))));
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpeg")
                .Returns(Result.From(renamedFileId));

            // Act
            Result<string> result = await _sut.SaveBookArtworkAsync(libraryId, bookId, libraryName, authorName, bookTitle, _artworkDtoFixture.Create(localPath: sourcePath, remoteUrl: "http://artwork.example/cover.jpg"), CancellationToken.None);

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
    public async Task SaveBookArtworkAsync_WhenRemoteDownloadIsCancelled_ShouldRethrowCancellation()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        async Task Act()
        {
            await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title", _artworkDtoFixture.Create(remoteUrl: "http://artwork.example/cover.jpg"), cancellationTokenSource.Token);
        }

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
    }

    [Fact]
    public void DeleteBookArtwork_WhenCalled_ShouldDeleteTheCoverFilesInTheBookDirectory()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        string libraryName = "Test Library";
        string authorName = "Test Author";
        string bookTitle = "Test Book";

        string artworkDirectoryPath = BuildArtworkDirectoryPath(libraryId, bookId, libraryName, authorName, bookTitle);
        MockArtworkDirectoryStubs(artworkDirectoryPath);
        FileSystemPathId coverFileId = _fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "cover.jpeg"));
        FileSystemPathId otherFileId = _fileSystemPathIdFixture.Create(Path.Combine(artworkDirectoryPath, "metadata.json"));
        _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From<IEnumerable<FileSystemPathId>>([coverFileId, otherFileId]));
        _mockFileProviderService.DeleteFile(Arg.Any<FileSystemPathId>()).Returns(Result.Deleted);

        // Act
        Result<Deleted> result = _sut.DeleteBookArtwork(libraryId, bookId, libraryName, authorName, bookTitle);

        // Assert
        Assert.False(result.IsFailure);
        _mockFileProviderService.Received(1).DeleteFile(coverFileId);
        _mockFileProviderService.DidNotReceive().DeleteFile(otherFileId);
    }

    [Fact]
    public void DeleteBookArtwork_WhenCalledWithInvalidNameSegments_ShouldReturnError()
    {
        // Arrange
        _mockPathService.SanitizeSegment(Arg.Any<string>())
            .Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        Result<Deleted> result = _sut.DeleteBookArtwork(Guid.NewGuid(), Guid.NewGuid(), "Library", "Author", "Title");

        // Assert
        Assert.True(result.IsFailure);
        _mockFileProviderService.DidNotReceive().GetFilePaths(Arg.Any<FileSystemPathId>(), true);
    }

    /// <summary>
    /// Stubs the path and file system services so that the artwork is stored into the given directory.
    /// </summary>
    /// <param name="artworkDirectoryPath">The file system path of the directory into which the artwork is stored.</param>
    private void MockArtworkDirectoryStubs(string artworkDirectoryPath)
    {
        string mediaRoot = Path.Combine(AppContext.BaseDirectory, "media");
        string booksPath = Path.Combine(mediaRoot, "books");
        _mockPathService.CombinePath(AppContext.BaseDirectory, "media").Returns(Result.From(mediaRoot));
        _mockPathService.CombinePath(mediaRoot, "books").Returns(Result.From(booksPath));

        _mockPathService.SanitizeSegment(Arg.Any<string>())
            .Returns(callInfo =>
            {
                string name = callInfo.Arg<string>();
                return Result.From(_pathSegmentFixture.Create(name: name, isDirectory: true, isDrive: false));
            });

        _mockPathService.CombinePath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1))));

        // every directory except the artwork directory itself already exists, so that only the final directory is created
        _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>())
            .Returns(callInfo => Result.From(!string.Equals(callInfo.Arg<FileSystemPathId>().Path, artworkDirectoryPath, StringComparison.OrdinalIgnoreCase)));
        _mockDirectoryProviderService.CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(_fileSystemPathIdFixture.Create(Path.Combine(callInfo.Arg<FileSystemPathId>().Path, callInfo.Arg<string>()))));

        _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true)
            .Returns(Result.From<IEnumerable<FileSystemPathId>>([]));
    }

    /// <summary>
    /// Builds the file system path of the directory that the artwork of the book is stored into.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="bookId">The Id of the book.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="authorName">The name of the author of the book.</param>
    /// <param name="bookTitle">The title of the book.</param>
    /// <returns>The expected file system path of the book artwork directory.</returns>
    private static string BuildArtworkDirectoryPath(Guid libraryId, Guid bookId, string libraryName, string authorName, string bookTitle)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "media",
            "books",
            $"{libraryName}-{libraryId}",
            authorName,
            $"{bookTitle}-{bookId}");
    }

    /// <summary>
    /// Creates a temporary image file with the given size and returns its path.
    /// </summary>
    /// <param name="smallImageBytes">The size of the temporary image file, in bytes.</param>
    /// <returns>The file system path of the created temporary image file.</returns>
    private static string CreateTempImageFile(int smallImageBytes)
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
            System.Diagnostics.ProcessStartInfo startInfo = OperatingSystem.IsWindows()
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

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo)!;
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
