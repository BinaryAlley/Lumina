#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;

/// <summary>
/// Contains unit tests for the <see cref="UserSettings"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsTests
{
    private readonly UserIdFixture _userIdFixture = new();
    private readonly UserSettingsFixture _userSettingsFixture = new();

    [Fact]
    public void Create_WhenCalledWithoutValues_ShouldCreateSettingsWithDefaultValues()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();

        // Act
        Result<UserSettings> result = UserSettings.Create(userId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(userId, result.Value.UserId);
        Assert.True(result.Value.IsPaginationEnabled);
        Assert.Equal(48, result.Value.ItemsPerPage);
        Assert.False(result.Value.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public void Create_WhenCalledWithoutUserId_ShouldCreateSettingsWithDefaultValuesAndGeneratedUserId()
    {
        // Act
        Result<UserSettings> result = UserSettings.Create();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value.UserId);
        Assert.True(result.Value.IsPaginationEnabled);
        Assert.Equal(48, result.Value.ItemsPerPage);
        Assert.False(result.Value.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldGenerateDistinctIds()
    {
        // Act
        Result<UserSettings> firstResult = UserSettings.Create();
        Result<UserSettings> secondResult = UserSettings.Create();

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        Assert.NotEqual(firstResult.Value.UserId.Value, secondResult.Value.UserId.Value);
        Assert.NotEqual(firstResult.Value.Id.Value, secondResult.Value.Id.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateSettings()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();

        // Act
        Result<UserSettings> result = UserSettings.Create(userId, isPaginationEnabled: false, itemsPerPage: 24, ignoreThePrefixForAlphaPicker: true);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(userId, result.Value.UserId);
        Assert.False(result.Value.IsPaginationEnabled);
        Assert.Equal(24, result.Value.ItemsPerPage);
        Assert.True(result.Value.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldGenerateDistinctSettingsIdFromUserId()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();

        // Act
        Result<UserSettings> result = UserSettings.Create(userId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(result.Value.Id.Value, result.Value.UserId.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenItemsPerPageIsNotPositive_ShouldReturnError(int itemsPerPage)
    {
        // Arrange
        UserId userId = _userIdFixture.Create();

        // Act
        Result<UserSettings> result = UserSettings.Create(userId, isPaginationEnabled: true, itemsPerPage: itemsPerPage, ignoreThePrefixForAlphaPicker: false);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero, result.FirstError);
    }

    [Fact]
    public void UpdateSettings_WhenCalledWithValidValues_ShouldUpdateSettings()
    {
        // Arrange
        UserSettings userSettings = _userSettingsFixture.Create();

        // Act
        Result<Updated> result = userSettings.UpdateSettings(isPaginationEnabled: false, itemsPerPage: 12, ignoreThePrefixForAlphaPicker: true);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(userSettings.IsPaginationEnabled);
        Assert.Equal(12, userSettings.ItemsPerPage);
        Assert.True(userSettings.IgnoreThePrefixForAlphaPicker);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdateSettings_WhenItemsPerPageIsNotPositive_ShouldReturnError(int itemsPerPage)
    {
        // Arrange
        UserSettings userSettings = _userSettingsFixture.Create();

        // Act
        Result<Updated> result = userSettings.UpdateSettings(isPaginationEnabled: true, itemsPerPage: itemsPerPage, ignoreThePrefixForAlphaPicker: false);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero, result.FirstError);
    }
}
