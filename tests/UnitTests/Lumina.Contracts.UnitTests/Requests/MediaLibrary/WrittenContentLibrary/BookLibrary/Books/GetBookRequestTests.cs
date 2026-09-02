#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
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
    private readonly GetBookRequestFixture _getBookRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetBookRequest()
    {
        // Act
        GetBookRequest sut = _getBookRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Id));
    }

    [Fact]
    public void Constructor_WhenPassingNullId_ShouldReturnNullId()
    {
        // Act
        GetBookRequest sut = new(Id: null);

        // Assert
        Assert.Null(sut.Id);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetBookRequest_ShouldPreserveValues()
    {
        // Arrange
        GetBookRequest expected = _getBookRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetBookRequest? actual = JsonSerializer.Deserialize<GetBookRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
