#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.ID3.Common.Models.DTO.Tags;
using Lumina.Plugins.ID3.Core.Tags;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Plugins.ID3.UnitTests.Core.Tags;

/// <summary>
/// Contains unit tests for the <see cref="Id3TagReader"/> class, covering the guard branches that do not reach the tagging library.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3TagReaderTests : IDisposable
{
    private readonly Id3TagReader _sut = new();
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Id3TagReaderTests"/> class.
    /// </summary>
    public Id3TagReaderTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-id3-reader-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public void Read_WhenPathIsNull_ShouldReturnNull()
    {
        // Act
        Id3TagDto? result = _sut.Read(null!);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")] // an empty path
    [InlineData("   ")] // a white space path
    public void Read_WhenPathIsEmptyOrWhiteSpace_ShouldReturnNull(string path)
    {
        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Read_WhenTheFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "missing.wav");

        // Act
        Id3TagDto? result = _sut.Read(path);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, recursive: true);
        GC.SuppressFinalize(this);
    }
}
