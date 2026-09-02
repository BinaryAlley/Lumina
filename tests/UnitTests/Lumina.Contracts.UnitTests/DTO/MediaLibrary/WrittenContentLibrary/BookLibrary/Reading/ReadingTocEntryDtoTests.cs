#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingTocEntryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingTocEntryDtoTests
{
    private readonly ReadingTocEntryDtoFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidReadingTocEntryDto()
    {
        // Act
        ReadingTocEntryDto sut = _fixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Label));
        Assert.False(string.IsNullOrWhiteSpace(sut.LocationRef));
    }

    [Fact]
    public void RoundTrip_WhenSerializingReadingTocEntryDto_ShouldPreserveValues()
    {
        // Arrange
        ReadingTocEntryDto expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingTocEntryDto? actual = JsonSerializer.Deserialize<ReadingTocEntryDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
}
