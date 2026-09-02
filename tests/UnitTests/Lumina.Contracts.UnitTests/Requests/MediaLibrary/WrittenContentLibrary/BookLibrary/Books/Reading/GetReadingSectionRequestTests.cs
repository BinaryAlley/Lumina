#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingSectionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionRequestTests
{
    private readonly GetReadingSectionRequestFixture _getReadingSectionRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetReadingSectionRequest()
    {
        // Act
        GetReadingSectionRequest sut = _getReadingSectionRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.BookId);
        Assert.False(string.IsNullOrWhiteSpace(sut.LocationRef));
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetReadingSectionRequest_ShouldPreserveValues()
    {
        // Arrange
        GetReadingSectionRequest expected = _getReadingSectionRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetReadingSectionRequest? actual = JsonSerializer.Deserialize<GetReadingSectionRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
