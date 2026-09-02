#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingAvailabilityRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityRequestTests
{
    private readonly GetReadingAvailabilityRequestFixture _getReadingAvailabilityRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetReadingAvailabilityRequest()
    {
        // Act
        GetReadingAvailabilityRequest sut = _getReadingAvailabilityRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.BookId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetReadingAvailabilityRequest_ShouldPreserveValues()
    {
        // Arrange
        GetReadingAvailabilityRequest expected = _getReadingAvailabilityRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetReadingAvailabilityRequest? actual = JsonSerializer.Deserialize<GetReadingAvailabilityRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
