#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingResourceDataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingResourceDataDtoTests
{
    private readonly ReadingResourceDataDtoFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidReadingResourceDataDto()
    {
        // Act
        ReadingResourceDataDto sut = _fixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEmpty(sut.Data);
        Assert.False(string.IsNullOrWhiteSpace(sut.MimeType));
    }

    [Fact]
    public void Create_WhenProvidedData_ShouldPreserveIt()
    {
        // Arrange
        byte[] data = Guid.NewGuid().ToByteArray();

        // Act
        ReadingResourceDataDto sut = _fixture.Create(data: data);

        // Assert
        Assert.Equal(data, sut.Data);
    }

    [Fact]
    public void RoundTrip_WhenSerializingReadingResourceDataDto_ShouldPreserveValues()
    {
        // Arrange
        ReadingResourceDataDto expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingResourceDataDto? actual = JsonSerializer.Deserialize<ReadingResourceDataDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
}
