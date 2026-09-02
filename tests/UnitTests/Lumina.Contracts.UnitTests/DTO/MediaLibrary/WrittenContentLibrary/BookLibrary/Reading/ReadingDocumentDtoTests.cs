#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingDocumentDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingDocumentDtoTests
{
    private readonly ReadingDocumentDtoFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidReadingDocumentDto()
    {
        // Act
        ReadingDocumentDto sut = _fixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Title));
        Assert.NotEmpty(sut.Spine);
        Assert.NotNull(sut.Resources);
    }

    [Fact]
    public void RoundTrip_WhenSerializingReadingDocumentDto_ShouldPreserveValues()
    {
        // Arrange
        ReadingDocumentDto expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingDocumentDto? actual = JsonSerializer.Deserialize<ReadingDocumentDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
}
