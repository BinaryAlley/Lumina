#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryBookReadersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersRequestTests
{
    private readonly GetLibraryBookReadersRequestFixture _getLibraryBookReadersRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetLibraryBookReadersRequest()
    {
        // Act
        GetLibraryBookReadersRequest sut = _getLibraryBookReadersRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetLibraryBookReadersRequest_ShouldPreserveValues()
    {
        // Arrange
        GetLibraryBookReadersRequest expected = _getLibraryBookReadersRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetLibraryBookReadersRequest? actual = JsonSerializer.Deserialize<GetLibraryBookReadersRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
