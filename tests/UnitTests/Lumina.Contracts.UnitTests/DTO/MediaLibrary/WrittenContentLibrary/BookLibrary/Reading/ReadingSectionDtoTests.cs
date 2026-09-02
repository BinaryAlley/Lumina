#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingSectionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingSectionDtoTests
{
    private readonly ReadingSectionDtoFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidReadingSectionDto()
    {
        // Act
        ReadingSectionDto sut = _fixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.LocationRef));
        Assert.False(string.IsNullOrWhiteSpace(sut.ContentHtml));
    }

    [Fact]
    public void RoundTrip_WhenSerializingReadingSectionDto_ShouldPreserveValues()
    {
        // Arrange
        ReadingSectionDto expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingSectionDto? actual = JsonSerializer.Deserialize<ReadingSectionDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
