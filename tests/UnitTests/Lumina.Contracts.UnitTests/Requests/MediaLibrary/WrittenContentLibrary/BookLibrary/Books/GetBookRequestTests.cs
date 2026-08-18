#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetBookRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookRequestTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingGetBookRequest_ShouldPreserveValues()
    {
        // Arrange
        GetBookRequest expected = new("bookId-123");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetBookRequest? actual = JsonSerializer.Deserialize<GetBookRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetBookRequestWithNullId_ShouldPreserveNull()
    {
        // Arrange
        GetBookRequest expected = new(null);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetBookRequest? actual = JsonSerializer.Deserialize<GetBookRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Id);
    }
}
