#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.FileSystemManagement;
using Lumina.Contracts.Fixtures.Core.DTO.FileSystemManagement;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.FileSystemManagement;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemItemDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemDtoTests
{
    private readonly FileSystemItemDtoFixture _fileSystemItemDtoFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidFileSystemItemDto()
    {
        // Act
        FileSystemItemDto sut = _fileSystemItemDtoFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
        Assert.False(string.IsNullOrWhiteSpace(sut.Name));
    }

    [Fact]
    public void RoundTrip_WhenSerializingFileSystemItem_ShouldPreserveValues()
    {
        // Arrange
        FileSystemItemDto expected = _fileSystemItemDtoFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        FileSystemItemDto? actual = JsonSerializer.Deserialize<FileSystemItemDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        FileSystemItemDto first = _fileSystemItemDtoFixture.Create();
        FileSystemItemDto second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
