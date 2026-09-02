#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingResourceInfoDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingResourceInfoDtoTests
{
    private readonly ReadingResourceInfoDtoFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidReadingResourceInfoDto()
    {
        // Act
        ReadingResourceInfoDto sut = _fixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.RelativeFilePath));
        Assert.False(string.IsNullOrWhiteSpace(sut.MimeType));
    }

    [Fact]
    public void RoundTrip_WhenSerializingReadingResourceInfoDto_ShouldPreserveValues()
    {
        // Arrange
        ReadingResourceInfoDto expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingResourceInfoDto? actual = JsonSerializer.Deserialize<ReadingResourceInfoDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
