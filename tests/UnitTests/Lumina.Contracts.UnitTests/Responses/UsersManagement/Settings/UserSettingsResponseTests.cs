#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.UsersManagement.Settings;
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
    private readonly UserSettingsResponseFixture _userSettingsResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUserSettingsResponse_ShouldPreserveValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserSettingsResponse expected = _userSettingsResponseFixture.Create(
            userId: userId,
            isPaginationEnabled: true,
            itemsPerPage: 50,
            shouldIgnoreThePrefixForAlphaPicker: true,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false
        );

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
        UserSettingsResponse sut = _userSettingsResponseFixture.Create(
            userId: userId,
            isPaginationEnabled: false,
            itemsPerPage: 25,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: false,
            shouldAggregateMetadataWhenMissing: false
        );

        // Act
        (Guid sutUserId, bool isPaginationEnabled, int itemsPerPage, bool shouldIgnoreThePrefixForAlphaPicker, bool isThemeCachingEnabled, bool shouldAggregateMetadataWhenMissing) = sut;

        // Assert
        Assert.Equal(sut.UserId, sutUserId);
        Assert.Equal(sut.IsPaginationEnabled, isPaginationEnabled);
        Assert.Equal(sut.ItemsPerPage, itemsPerPage);
        Assert.Equal(sut.ShouldIgnoreThePrefixForAlphaPicker, shouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(sut.IsThemeCachingEnabled, isThemeCachingEnabled);
        Assert.Equal(sut.ShouldAggregateMetadataWhenMissing, shouldAggregateMetadataWhenMissing);
    }
}
