#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationRequestTests
{
    private readonly GetAuthorizationRequestFixture _getAuthorizationRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetAuthorizationRequest()
    {
        // Act
        GetAuthorizationRequest sut = _getAuthorizationRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.True(sut.UserId.HasValue);
    }

    [Fact]
    public void Constructor_WhenPassingNullUserId_ShouldReturnNullUserId()
    {
        // Act
        GetAuthorizationRequest sut = new(UserId: null);

        // Assert
        Assert.Null(sut.UserId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetAuthorizationRequest_ShouldPreserveValues()
    {
        // Arrange
        GetAuthorizationRequest expected = _getAuthorizationRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetAuthorizationRequest? actual = JsonSerializer.Deserialize<GetAuthorizationRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
