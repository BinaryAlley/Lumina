#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Fixtures.Common.Enums.PhotoLibrary;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using NSubstitute;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="ThumbnailService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailServiceTests
{
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly ThumbnailService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailServiceTests"/> class.
    /// </summary>
    public ThumbnailServiceTests()
    {
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _sut = new ThumbnailService(_mockEnvironmentContext);
    }

    [Fact]
    public async Task GetThumbnailAsync_WithValidPath_ShouldReturnThumbnail()
    {
        // Arrange
        string path = @"C:\TestImage.jpg";
        int quality = 80;
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

        // Create a small valid JPEG image
        byte[] imageBytes;
        using (Image<Rgba32> image = new(10, 10))
        {
            using MemoryStream ms = new();
            image.Save(ms, new JpegEncoder());
            imageBytes = ms.ToArray();
        }

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.JPEG));
        _mockEnvironmentContext.FileProviderService.GetFileAsync(pathId)
            .Returns(Result.From(imageBytes));

        // Act
        Result<Thumbnail> result = await _sut.GetThumbnailAsync(path, quality, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ImageType.JPEG, result.Value.Type);
        Assert.NotEmpty(result.Value.Bytes);
    }

    [Fact]
    public async Task GetThumbnailAsync_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        int quality = 80;

        // Act
        Result<Thumbnail> result = await _sut.GetThumbnailAsync(invalidPath, quality, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public async Task GetThumbnailAsync_WhenFileTypeServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(@"C:\TestImage.jpg");
        int quality = 80;

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public async Task GetThumbnailAsync_WhenImageTypeIsNone_ShouldReturnNoThumbnailError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(@"C:\TestFile.txt");
        int quality = 80;

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.None));

        // Act
        Result<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Thumbnails.NoThumbnail, result.FirstError);
    }

    [Fact]
    public async Task GetThumbnailAsync_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(@"C:\TestImage.jpg");
        int quality = 80;

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(Result.From(ImageType.JPEG));
        _mockEnvironmentContext.FileProviderService.GetFileAsync(pathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Theory]
    [ClassData(typeof(ImageTypeFixture))]
    public async Task GetThumbnailAsync_WithDifferentImageTypes_ShouldReturnThumbnail(ImageType imageType, byte[] imageBytes)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(@"C:\TestImage");
        int quality = 80;

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(Result.From(imageType));
        _mockEnvironmentContext.FileProviderService.GetFileAsync(pathId)
            .Returns(Result.From(imageBytes));

        // Act
        Result<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(imageType, result.Value.Type);
        Assert.NotEmpty(result.Value.Bytes);

        using (MemoryStream ms = new(result.Value.Bytes))
        {
            Image loadedImage = Image.Load(ms);
            Assert.NotNull(loadedImage);
        }
    }

    [Theory]
    [InlineData(ImageType.JPEG2000)] // resolution adjustment switch branch for JPEG2000
    [InlineData(ImageType.JPEG_CANON)] // resolution adjustment switch branch for Canon JPEG
    [InlineData(ImageType.JPEG_UNKNOWN)] // resolution adjustment switch branch for unknown JPEG
    [InlineData(ImageType.TIFF)] // resolution adjustment switch branch for TIFF
    [InlineData(ImageType.WEBP)] // resolution adjustment switch branch for WEBP
    [InlineData(ImageType.TGA)] // resolution adjustment switch branch for TGA
    public async Task GetThumbnailAsync_WithAdditionalSupportedImageTypes_ShouldReturnAdjustedThumbnail(ImageType imageType)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(@"C:\TestImage");
        int quality = 80;
        byte[] imageBytes = CreateImageBytes(new JpegEncoder());

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(Result.From(imageType));
        _mockEnvironmentContext.FileProviderService.GetFileAsync(pathId)
            .Returns(Result.From(imageBytes));

        // Act
        Result<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(imageType, result.Value.Type);
        Assert.NotEmpty(result.Value.Bytes);

        using (MemoryStream ms = new(result.Value.Bytes))
        {
            Image loadedImage = Image.Load(ms);
            Assert.NotNull(loadedImage);
        }
    }

    [Theory]
    [InlineData(ImageType.SVG)] // SVG is not resized, original bytes are returned
    [InlineData(ImageType.PICT)] // PICT is not resized, original bytes are returned
    [InlineData(ImageType.ICO)] // ICO is not resized, original bytes are returned
    [InlineData(ImageType.PSD)] // PSD is not resized, original bytes are returned
    [InlineData(ImageType.AVIF)] // AVIF is not resized, original bytes are returned
    public async Task GetThumbnailAsync_WithUnsupportedNonNoneImageType_ShouldReturnOriginalBytes(ImageType imageType)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(@"C:\TestImage");
        int quality = 80;
        byte[] imageBytes = CreateImageBytes(new JpegEncoder());

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, Arg.Any<CancellationToken>())
            .Returns(Result.From(imageType));
        _mockEnvironmentContext.FileProviderService.GetFileAsync(pathId)
            .Returns(Result.From(imageBytes));

        // Act
        Result<Thumbnail> result = await _sut.GetThumbnailAsync(pathId, quality, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(imageType, result.Value.Type);
        Assert.Equal(imageBytes, result.Value.Bytes);
    }

    [Fact]
    public async Task GetThumbnailAsync_WithCancellation_ShouldThrowTaskCanceledException()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(@"C:\TestImage.jpg");
        int quality = 80;
        CancellationTokenSource cts = new();
        cts.Cancel();

        _mockEnvironmentContext.FileTypeService.GetImageTypeAsync(pathId, cts.Token)
            .Returns(Task.FromCanceled<Result<ImageType>>(cts.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _sut.GetThumbnailAsync(pathId, quality, cts.Token));
    }

    /// <summary>
    /// Creates the bytes of a small image encoded with the specified encoder.
    /// </summary>
    /// <param name="encoder">The encoder used to encode the image.</param>
    /// <returns>The bytes of the created image.</returns>
    private static byte[] CreateImageBytes(IImageEncoder encoder)
    {
        using Image<Rgba32> image = new(10, 10);
        using MemoryStream memoryStream = new();
        image.Save(memoryStream, encoder);
        return memoryStream.ToArray();
    }
}
