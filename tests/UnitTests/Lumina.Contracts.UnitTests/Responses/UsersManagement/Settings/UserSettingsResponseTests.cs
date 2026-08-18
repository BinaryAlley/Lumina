#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.UsersManagement.Settings;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.UsersManagement.Settings;

/// <summary>
/// Contains unit tests for the <see cref="UserSettingsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUserSettingsResponse_ShouldPreserveValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserSettingsResponse expected = new(userId, true, 50, true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UserSettingsResponse? actual = JsonSerializer.Deserialize<UserSettingsResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserSettingsResponse sut = new(userId, false, 25, false);

        // Act
        (Guid sutUserId, bool isPaginationEnabled, int itemsPerPage, bool ignoreThePrefixForAlphaPicker) = sut;

        // Assert
        Assert.Equal(sut.UserId, sutUserId);
        Assert.Equal(sut.IsPaginationEnabled, isPaginationEnabled);
        Assert.Equal(sut.ItemsPerPage, itemsPerPage);
        Assert.Equal(sut.IgnoreThePrefixForAlphaPicker, ignoreThePrefixForAlphaPicker);
    }
}
