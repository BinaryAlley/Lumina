#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Services.Fixtures;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using NSubstitute;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="ThumbnailService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailServiceTests
{
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly ThumbnailService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailServiceTests"/> class.
    /// </summary>
    public ThumbnailServiceTests()
    {
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _sut = new ThumbnailService(_mockEnvironmentContext);
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
    }

    [Fact]
    public async Task GetThumbnailAsync_WithValidPath_ShouldReturnThumbnail()
    {
        // Arrange
        string path = @"C:\TestImage.jpg";
        int quality = 80;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Create a small valid JPEG image
        byte[] imageBytes;
        using (Image<Rgba32> image = new(10, 10))
        {
            using MemoryStream ms = new();
            image.Save(ms, new JpegEncoder());
            imageBytes = ms.ToArray();
        }

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(ImageType.JPEG));
        _mockEnvironmentContext.FileProviderService.GetFileAsync(pathId)
            .Returns(ErrorOrFactory.From(imageBytes));

        // Act
        ErrorOr<Thumbnail> result = await _sut.GetThumbnailAsync(path, quality, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Type.Should().Be(ImageType.JPEG);
        result.Value.Bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetThumbnailAsync_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        int quality = 80;

        // Act
        ErrorOr<Thumbnail> result = await _sut.GetThumbnailAsync(invalidPath, quality, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public async Task GetThumbnailAsync_WhenFileTypeServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestImage.jpg");
        int quality = 80;

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public async Task GetThumbnailAsync_WhenImageTypeIsNone_ShouldReturnNoThumbnailError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestFile.txt");
        int quality = 80;

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(ImageType.None));

        // Act
        ErrorOr<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Thumbnails.NoThumbnail);
    }

    [Fact]
    public async Task GetThumbnailAsync_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestImage.jpg");
        int quality = 80;

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(ImageType.JPEG));
        _mockEnvironmentContext.FileProviderService.GetFileAsync(pathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Theory]
    [ClassData(typeof(ImageTypeFixture))]
    public async Task GetThumbnailAsync_WithDifferentImageTypes_ShouldReturnThumbnail(ImageType imageType, byte[] imageBytes)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestImage");
        int quality = 80;

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(imageType));
        _mockEnvironmentContext.FileProviderService.GetFileAsync(pathId)
            .Returns(ErrorOrFactory.From(imageBytes));

        // Act
        ErrorOr<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Type.Should().Be(imageType);
        result.Value.Bytes.Should().NotBeEmpty();

        using (MemoryStream ms = new(result.Value.Bytes))
        {
            Image loadedImage = Image.Load(ms);
            loadedImage.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetThumbnailAsync_WithCancellation_ShouldThrowTaskCanceledException()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestImage.jpg");
        int quality = 80;
        CancellationTokenSource cts = new();
        cts.Cancel();

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, cts.Token)
            .Returns(Task.FromCanceled<ErrorOr<ImageType>>(cts.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _sut.GetThumbnailAsync(pathId, quality, cts.Token));
    }
}
