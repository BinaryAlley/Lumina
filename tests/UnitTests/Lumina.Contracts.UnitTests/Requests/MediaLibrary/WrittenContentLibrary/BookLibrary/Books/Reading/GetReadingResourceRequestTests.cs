#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingResourceRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceRequestTests
{
    private readonly GetReadingResourceRequestFixture _getReadingResourceRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetReadingResourceRequest()
    {
        // Act
        GetReadingResourceRequest sut = _getReadingResourceRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.BookId);
        Assert.False(string.IsNullOrWhiteSpace(sut.ResourceKey));
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetReadingResourceRequest_ShouldPreserveValues()
    {
        // Arrange
        GetReadingResourceRequest expected = _getReadingResourceRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetReadingResourceRequest? actual = JsonSerializer.Deserialize<GetReadingResourceRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
