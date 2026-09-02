#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingSpineItemDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingSpineItemDtoTests
{
    private readonly ReadingSpineItemDtoFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidReadingSpineItemDto()
    {
        // Act
        ReadingSpineItemDto sut = _fixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.LocationRef));
        Assert.False(string.IsNullOrWhiteSpace(sut.RelativeSectionFilePath));
    }

    [Fact]
    public void RoundTrip_WhenSerializingReadingSpineItemDto_ShouldPreserveValues()
    {
        // Arrange
        ReadingSpineItemDto expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingSpineItemDto? actual = JsonSerializer.Deserialize<ReadingSpineItemDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
