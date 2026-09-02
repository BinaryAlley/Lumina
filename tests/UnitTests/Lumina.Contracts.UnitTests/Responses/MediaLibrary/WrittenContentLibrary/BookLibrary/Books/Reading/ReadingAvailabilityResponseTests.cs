#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingAvailabilityResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingAvailabilityResponseTests
{
    private readonly ReadingAvailabilityResponseFixture _readingAvailabilityResponseFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidReadingAvailabilityResponse()
    {
        // Act
        ReadingAvailabilityResponse sut = _readingAvailabilityResponseFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(System.Guid.Empty, sut.BookId);
        Assert.NotEqual(System.Guid.Empty, sut.LibraryId);
        Assert.True(sut.IsAvailable);
    }

    [Fact]
    public void RoundTrip_WhenSerializingReadingAvailabilityResponse_ShouldPreserveValues()
    {
        // Arrange
        ReadingAvailabilityResponse expected = _readingAvailabilityResponseFixture.Create(isAvailable: true, errorCode: null);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingAvailabilityResponse? actual = JsonSerializer.Deserialize<ReadingAvailabilityResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingReadingAvailabilityResponseWithErrorCode_ShouldPreserveErrorCode()
    {
        // Arrange
        ReadingAvailabilityResponse expected = _readingAvailabilityResponseFixture.Create(isAvailable: false, errorCode: "ReaderDisabled");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReadingAvailabilityResponse? actual = JsonSerializer.Deserialize<ReadingAvailabilityResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
        Assert.False(actual.IsAvailable);
        Assert.Equal("ReaderDisabled", actual.ErrorCode);
    }
}
