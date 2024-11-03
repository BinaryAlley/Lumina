#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Entities.Fixtures;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="FileTypeService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileTypeServiceTests
{
    private MockFileSystem _mockFileSystem = null!;
    private readonly IFileSystemPermissionsService _mockFileSystemPermissionsService;
    private FileTypeService? sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private readonly FileFixture _fileFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTypeServiceTests"/> class.
    /// </summary>
    public FileTypeServiceTests()
    {
        _mockFileSystemPermissionsService = Substitute.For<IFileSystemPermissionsService>();
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
        _fileFixture = new FileFixture();
    }

    [Fact]
    public async Task GetImageTypeAsync_WithValidFile_ShouldReturnCorrectImageType()
    {
        // Arrange
        File file = _fileFixture.CreateFile();
        byte[] pngHeader = [137, 80, 78, 71, 13, 10, 26, 10];
        byte[] fileContent = [.. pngHeader, .. Enumerable.Repeat<byte>(0, 100)];

        SetupMockFileSystem(file.Id, fileContent);
        sut = new FileTypeService(_mockFileSystem, _mockFileSystemPermissionsService);
        _mockFileSystemPermissionsService.CanAccessPath(file.Id, FileAccessMode.ReadContents).Returns(true);

        // Act
        ErrorOr<ImageType> result = await sut.GetImageTypeAsync(file, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(ImageType.PNG);
    }

    [Fact]
    public async Task GetImageTypeAsync_WithUnauthorizedAccess_ShouldReturnError()
    {
        // Arrange
        File file = _fileFixture.CreateFile();

        SetupMockFileSystem(file.Id, []);
        sut = new FileTypeService(_mockFileSystem, _mockFileSystemPermissionsService);

        _mockFileSystemPermissionsService.CanAccessPath(file.Id, FileAccessMode.ReadContents).Returns(false);

        // Act
        ErrorOr<ImageType> result = await sut.GetImageTypeAsync(file, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public async Task GetImageTypeAsync_WithSmallFile_ShouldReturnNone()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\test.jpg");
        byte[] fileContent = [1, 2, 3]; // Less than BUFFER_SIZE

        SetupMockFileSystem(pathId, fileContent);
        sut = new FileTypeService(_mockFileSystem, _mockFileSystemPermissionsService);
        _mockFileSystemPermissionsService.CanAccessPath(pathId, FileAccessMode.ReadContents).Returns(true);

        // Act
        ErrorOr<ImageType> result = await sut.GetImageTypeAsync(pathId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(ImageType.None);
    }

    [Fact]
    public async Task GetImageTypeAsync_WithSvgFile_ShouldReturnSvgType()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\test.svg");
        string svgContent = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";
        byte[] fileContent = Encoding.UTF8.GetBytes(svgContent);

        SetupMockFileSystem(pathId, fileContent);
        sut = new FileTypeService(_mockFileSystem, _mockFileSystemPermissionsService);
        _mockFileSystemPermissionsService.CanAccessPath(pathId, FileAccessMode.ReadContents).Returns(true);

        // Act
        ErrorOr<ImageType> result = await sut.GetImageTypeAsync(pathId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(ImageType.SVG);
    }

    [Fact]
    public async Task GetImageTypeAsync_WithTgaFile_ShouldReturnTgaType()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\test.tga");
        byte[] tgaHeader = new byte[18];
        tgaHeader[2] = 2; // Set image type to 2 (uncompressed true-color image)
        byte[] fileContent = [.. tgaHeader, .. Enumerable.Repeat<byte>(0, 100)];

        SetupMockFileSystem(pathId, fileContent);
        sut = new FileTypeService(_mockFileSystem, _mockFileSystemPermissionsService);

        _mockFileSystemPermissionsService.CanAccessPath(pathId, FileAccessMode.ReadContents).Returns(true);

        // Act
        ErrorOr<ImageType> result = await sut.GetImageTypeAsync(pathId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(ImageType.TGA);
    }

    [Fact]
    public async Task GetImageTypeAsync_WithUnknownImageType_ShouldReturnNone()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\test.unknown");
        byte[] fileContent = Enumerable.Repeat<byte>(0, 100).ToArray();

        SetupMockFileSystem(pathId, fileContent);
        sut = new FileTypeService(_mockFileSystem, _mockFileSystemPermissionsService);
        _mockFileSystemPermissionsService.CanAccessPath(pathId, FileAccessMode.ReadContents).Returns(true);

        // Act
        ErrorOr<ImageType> result = await sut.GetImageTypeAsync(pathId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(ImageType.None);
    }

    private void SetupMockFileSystem(FileSystemPathId pathId, byte[] fileContent)
    {
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { pathId.Path, new MockFileData(fileContent) }
        });

        _mockFileSystem = mockFileSystem;
    }

    [Fact]
    public async Task GetImageTypeAsync_WithSvgFileStartingWithXmlDeclaration_ShouldReturnSvgType()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\test.svg");
        string svgContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";
        byte[] fileContent = Encoding.UTF8.GetBytes(svgContent);

        SetupMockFileSystem(pathId, fileContent);
        sut = new FileTypeService(_mockFileSystem, _mockFileSystemPermissionsService);

        _mockFileSystemPermissionsService.CanAccessPath(pathId, FileAccessMode.ReadContents).Returns(true);

        // Act
        ErrorOr<ImageType> result = await sut.GetImageTypeAsync(pathId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(ImageType.SVG);
    }

    [Theory]
    [InlineData(new byte[] { 66, 77 }, ImageType.BMP, "BMP")]
    [InlineData(new byte[] { 71, 73, 70 }, ImageType.GIF, "GIF")]
    [InlineData(new byte[] { 137, 80, 78, 71 }, ImageType.PNG, "PNG")]
    [InlineData(new byte[] { 73, 73, 42 }, ImageType.TIFF, "TIFF (Little Endian)")]
    [InlineData(new byte[] { 77, 77, 42 }, ImageType.TIFF, "TIFF (Big Endian)")]
    [InlineData(new byte[] { 255, 216, 255, 224 }, ImageType.JPEG, "JPEG")]
    [InlineData(new byte[] { 255, 216, 255, 225 }, ImageType.JPEG_CANON, "JPEG (Canon)")]
    [InlineData(new byte[] { 255, 216, 255, 226 }, ImageType.JPEG_UNKNOWN, "JPEG (Unknown)")]
    [InlineData(new byte[] { 0x00, 0x11, 0x02, 0xFF }, ImageType.PICT, "PICT")]
    [InlineData(new byte[] { 0x00, 0x00, 0x01, 0x00 }, ImageType.ICO, "ICO")]
    [InlineData(new byte[] { 0x38, 0x42, 0x50, 0x53 }, ImageType.PSD, "PSD")]
    [InlineData(new byte[] { 0xFF, 0x4F, 0xFF, 0x51 }, ImageType.JPEG2000, "JPEG 2000")]
    [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x70, 0x20, 0x20 }, ImageType.AVIF, "AVIF")]
    [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20 }, ImageType.JPEG2000, "JPEG 2000 (variant)")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 }, ImageType.WEBP, "WEBP")]
    [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00 }, ImageType.None, "Unknown")]
    public async Task GetImageTypeAsync_WithVariousImageTypes_ShouldIdentifyCorrectly(byte[] header, ImageType expectedType, string testName)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId($@"C:\test.{testName.ToLower()}");
        byte[] fileContent = [.. header, .. Enumerable.Repeat<byte>(0, 100)];

        SetupMockFileSystem(pathId, fileContent);
        sut = new FileTypeService(_mockFileSystem, _mockFileSystemPermissionsService);

        _mockFileSystemPermissionsService.CanAccessPath(pathId, FileAccessMode.ReadContents).Returns(true);

        // Act
        ErrorOr<ImageType> result = await sut.GetImageTypeAsync(pathId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedType, because: $"the file header matches {testName}");
    }
}
