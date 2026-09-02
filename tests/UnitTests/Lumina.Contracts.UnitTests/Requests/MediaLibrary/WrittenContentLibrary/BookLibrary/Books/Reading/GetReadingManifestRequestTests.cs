#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingManifestRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestRequestTests
{
    private readonly GetReadingManifestRequestFixture _getReadingManifestRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetReadingManifestRequest()
    {
        // Act
        GetReadingManifestRequest sut = _getReadingManifestRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.BookId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetReadingManifestRequest_ShouldPreserveValues()
    {
        // Arrange
        GetReadingManifestRequest expected = _getReadingManifestRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetReadingManifestRequest? actual = JsonSerializer.Deserialize<GetReadingManifestRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
