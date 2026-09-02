#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingTocEntryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingTocEntryResponseTests
{
    private readonly ReadingTocEntryResponseFixture _fixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingReadingTocEntryResponse_ShouldPreserveValues()
    {
        // Arrange
        ReadingTocEntryResponse expected = _fixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingTocEntryResponse? actual = JsonSerializer.Deserialize<ReadingTocEntryResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
}
