#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.UsersManagement.Users;
using Lumina.Contracts.Responses.UsersManagement.Users;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.UsersManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserResponseTests
{
    private readonly UserResponseFixture _userResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUserResponse_ShouldPreserveValues()
    {
        // Arrange
        UserResponse expected = _userResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UserResponse? actual = JsonSerializer.Deserialize<UserResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingUserResponseWithUpdatedOnUtc_ShouldPreserveValue()
    {
        // Arrange
        DateTime updatedOnUtc = DateTime.UtcNow.AddDays(1);
        UserResponse expected = _userResponseFixture.Create(updatedOnUtc: updatedOnUtc);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UserResponse? actual = JsonSerializer.Deserialize<UserResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(updatedOnUtc, actual.UpdatedOnUtc);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        UserResponse sut = _userResponseFixture.Create();

        // Act
        (Guid sutId, string username, DateTime sutCreatedOnUtc, DateTime? sutUpdatedOnUtc) = sut;

        // Assert
        Assert.Equal(sut.Id, sutId);
        Assert.Equal(sut.Username, username);
        Assert.Equal(sut.CreatedOnUtc, sutCreatedOnUtc);
        Assert.Equal(sut.UpdatedOnUtc, sutUpdatedOnUtc);
    }
}
