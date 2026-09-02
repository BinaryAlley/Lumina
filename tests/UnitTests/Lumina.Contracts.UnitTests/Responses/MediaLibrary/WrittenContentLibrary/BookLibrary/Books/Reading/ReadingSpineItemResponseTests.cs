#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingSpineItemResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingSpineItemResponseTests
{
    private readonly ReadingSpineItemResponseFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingReadingSpineItemResponse_ShouldPreserveValues()
    {
        // Arrange
        ReadingSpineItemResponse expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingSpineItemResponse? actual = JsonSerializer.Deserialize<ReadingSpineItemResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
