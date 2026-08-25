#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Artwork;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Lumina.Infrastructure.Common.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.MediaLibrary.Artwork;

/// <summary>
/// Contains security tests for the <see cref="IBookArtworkService"/> contract, exercised through the real <see cref="BookArtworkService"/> implementation resolved from the dependency injection container.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookArtworkServiceSecurityTests
{
    private readonly IFileTypeService _mockFileTypeService;
    private readonly IFileProviderService _mockFileProviderService;
    private readonly IDirectoryProviderService _mockDirectoryProviderService;
    private readonly IBookArtworkService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly ArtworkDtoFixture _artworkDtoFixture = new();
    private readonly MediaSettingsDtoFixture _mediaSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookArtworkServiceSecurityTests"/> class.
    /// </summary>
    public BookArtworkServiceSecurityTests()
    {
        IEnvironmentContext mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockFileTypeService = Substitute.For<IFileTypeService>();
        _mockFileProviderService = Substitute.For<IFileProviderService>();
        _mockDirectoryProviderService = Substitute.For<IDirectoryProviderService>();
        mockEnvironmentContext.FileTypeService.Returns(_mockFileTypeService);
        mockEnvironmentContext.FileProviderService.Returns(_mockFileProviderService);
        mockEnvironmentContext.DirectoryProviderService.Returns(_mockDirectoryProviderService);

        MediaSettingsDto mediaSettings = _mediaSettingsDtoFixture.Create(rootDirectory: "media", librariesDirectory: "libraries", booksDirectory: "books");

        IConfiguration mockConfiguration = Substitute.For<IConfiguration>();
        mockConfiguration.GetSection(Arg.Any<string>()).Returns(Substitute.For<IConfigurationSection>());

        ServiceCollection services = new();
        services.AddInfrastructureLayerServices(mockConfiguration);
        services.AddScoped(_ => mockEnvironmentContext);
        // the real path service is used, so that the sanitization of hostile segment names is exercised
        services.AddScoped(_ => CreateRealPathService());
        services.AddSingleton(Options.Create(mediaSettings));
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();
        _sut = scope.ServiceProvider.GetRequiredService<IBookArtworkService>();
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenLocalArtworkIsAReparsePoint_ShouldRejectIt()
    {
        // Arrange
        string targetDirectory = Path.Combine(Path.GetTempPath(), $"lumina-security-junction-target-{Guid.NewGuid():N}");
        Directory.CreateDirectory(targetDirectory);
        string linkPath = Path.Combine(Path.GetTempPath(), $"lumina-security-junction-link-{Guid.NewGuid():N}");
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
    public async Task SaveBookArtworkAsync_WhenNameSegmentsContainPathTraversal_ShouldSanitizeThemIntoLiteralSegments()
    {
        // Arrange
        string sourcePath = Path.Combine(Path.GetTempPath(), $"lumina-security-source-{Guid.NewGuid():N}.jpg");
        File.WriteAllBytes(sourcePath, new byte[1024]);
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));
            _mockDirectoryProviderService.DirectoryExists(Arg.Any<FileSystemPathId>())
                .Returns(Result.From(true));
            _mockFileProviderService.GetFilePaths(Arg.Any<FileSystemPathId>(), true)
                .Returns(Result.From<IEnumerable<FileSystemPathId>>([]));

            string capturedDestinationPath = string.Empty;
            _mockFileProviderService.CopyFile(Arg.Any<FileSystemPathId>(), Arg.Do<FileSystemPathId>(destination => capturedDestinationPath = destination.Path), true)
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(AppContext.BaseDirectory, "media", "books", "cover.jpg"))));
            _mockFileProviderService.RenameFile(Arg.Any<FileSystemPathId>(), "cover.jpeg")
                .Returns(Result.From(_fileSystemPathIdFixture.Create(Path.Combine(AppContext.BaseDirectory, "media", "books", "cover.jpeg"))));

            // Act
            char separator = Path.DirectorySeparatorChar;
            Result<string> result = await _sut.SaveBookArtworkAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                libraryName: $"..{separator}..{separator}Library",
                authorName: $"..{separator}evil",
                bookTitle: $"..{separator}..{separator}cover",
                _artworkDtoFixture.Create(localPath: sourcePath),
                CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            Assert.False(string.IsNullOrEmpty(capturedDestinationPath));
            string[] segments = capturedDestinationPath.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
            Assert.DoesNotContain(segments, segment => segment == "..");
            Assert.DoesNotContain(segments, segment => segment == ".");
            Assert.DoesNotContain(result.Value.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries), segment => segment == "..");
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task SaveBookArtworkAsync_WhenLibraryNameIsADotDotSegment_ShouldReturnInvalidPath()
    {
        // Arrange
        string sourcePath = Path.Combine(Path.GetTempPath(), $"lumina-security-source-{Guid.NewGuid():N}.jpg");
        File.WriteAllBytes(sourcePath, new byte[1024]);
        try
        {
            _mockFileProviderService.FileExists(Arg.Any<FileSystemPathId>()).Returns(Result.From(true));
            _mockFileTypeService.GetImageTypeAsync(Arg.Any<FileSystemPathId>(), Arg.Any<CancellationToken>())
                .Returns(Result.From(ImageType.JPEG));

            // Act
            Result<string> result = await _sut.SaveBookArtworkAsync(Guid.NewGuid(), Guid.NewGuid(), libraryName: "..", authorName: "Author", bookTitle: "Title", _artworkDtoFixture.Create(localPath: sourcePath), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            _mockFileProviderService.DidNotReceive().CopyFile(Arg.Any<FileSystemPathId>(), Arg.Any<FileSystemPathId>(), true);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    /// <summary>
    /// Creates a real <see cref="PathService"/> backed by a stubbed path strategy, so that the sanitization of hostile segment names is exercised.
    /// </summary>
    /// <returns>The created real path service.</returns>
    private static IPathService CreateRealPathService()
    {
        IPathStrategy mockPathStrategy = Substitute.For<IPathStrategy>();
        mockPathStrategy.PathSeparator.Returns(Path.DirectorySeparatorChar);
        mockPathStrategy.GetInvalidPathSegmentCharsForPlatform().Returns(Path.GetInvalidFileNameChars());
        mockPathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(callInfo => Result.From(FileSystemPathId.Create(Path.Combine(callInfo.Arg<FileSystemPathId>().Path, callInfo.Arg<string>())).Value));
        mockPathStrategy.IsValidPath(Arg.Any<FileSystemPathId>()).Returns(true);

        IPlatformContext mockPlatformContext = Substitute.For<IPlatformContext>();
        mockPlatformContext.PathStrategy.Returns(mockPathStrategy);
        IPlatformContextManager mockPlatformContextManager = Substitute.For<IPlatformContextManager>();
        mockPlatformContextManager.GetCurrentContext().Returns(mockPlatformContext);

        return new PathService(mockPlatformContextManager);
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
