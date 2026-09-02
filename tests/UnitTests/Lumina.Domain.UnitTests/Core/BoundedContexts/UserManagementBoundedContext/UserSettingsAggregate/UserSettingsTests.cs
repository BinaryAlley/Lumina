#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
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
    private readonly UserSettingsIdFixture _userSettingsIdFixture = new();

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
        Assert.False(result.Value.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.False(result.Value.ShouldAggregateMetadataWhenMissing);
        Assert.False(result.Value.ShouldRenderPdfAsImages);
        Assert.True(result.Value.ShouldPreserveBookStyles);
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
        Assert.False(result.Value.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.False(result.Value.ShouldAggregateMetadataWhenMissing);
        Assert.False(result.Value.ShouldRenderPdfAsImages);
        Assert.True(result.Value.ShouldPreserveBookStyles);
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
        Result<UserSettings> result = UserSettings.Create(userId, isPaginationEnabled: false, itemsPerPage: 24, shouldIgnoreThePrefixForAlphaPicker: true, isThemeCachingEnabled: true, shouldAggregateMetadataWhenMissing: true, shouldRenderPdfAsImages: true, shouldPreserveBookStyles: true);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(userId, result.Value.UserId);
        Assert.False(result.Value.IsPaginationEnabled);
        Assert.Equal(24, result.Value.ItemsPerPage);
        Assert.True(result.Value.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.True(result.Value.IsThemeCachingEnabled);
        Assert.True(result.Value.ShouldAggregateMetadataWhenMissing);
        Assert.True(result.Value.ShouldRenderPdfAsImages);
        Assert.True(result.Value.ShouldPreserveBookStyles);
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
        Result<UserSettings> result = UserSettings.Create(userId, isPaginationEnabled: true, itemsPerPage: itemsPerPage, shouldIgnoreThePrefixForAlphaPicker: false, isThemeCachingEnabled: true, shouldAggregateMetadataWhenMissing: false, shouldRenderPdfAsImages: false, shouldPreserveBookStyles: false);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingIdAndValidValues_ShouldCreateSettingsWithThatId()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        UserSettingsId id = _userSettingsIdFixture.Create();

        // Act
        Result<UserSettings> result = UserSettings.Create(id, userId, isPaginationEnabled: true, itemsPerPage: 24, shouldIgnoreThePrefixForAlphaPicker: false, isThemeCachingEnabled: true, shouldAggregateMetadataWhenMissing: false, shouldRenderPdfAsImages: false, shouldPreserveBookStyles: true);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(24, result.Value.ItemsPerPage);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingIdAndNonPositiveItemsPerPage_ShouldReturnError()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        UserSettingsId id = _userSettingsIdFixture.Create();

        // Act
        Result<UserSettings> result = UserSettings.Create(id, userId, isPaginationEnabled: true, itemsPerPage: 0, shouldIgnoreThePrefixForAlphaPicker: false, isThemeCachingEnabled: true, shouldAggregateMetadataWhenMissing: false, shouldRenderPdfAsImages: false, shouldPreserveBookStyles: false);

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
        Result<Updated> result = userSettings.UpdateSettings(isPaginationEnabled: false, itemsPerPage: 12, shouldIgnoreThePrefixForAlphaPicker: true, isThemeCachingEnabled: false, shouldAggregateMetadataWhenMissing: true, shouldRenderPdfAsImages: true, shouldPreserveBookStyles: true);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(userSettings.IsPaginationEnabled);
        Assert.Equal(12, userSettings.ItemsPerPage);
        Assert.True(userSettings.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.False(userSettings.IsThemeCachingEnabled);
        Assert.True(userSettings.ShouldAggregateMetadataWhenMissing);
        Assert.True(userSettings.ShouldRenderPdfAsImages);
        Assert.True(userSettings.ShouldPreserveBookStyles);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdateSettings_WhenItemsPerPageIsNotPositive_ShouldReturnError(int itemsPerPage)
    {
        // Arrange
        UserSettings userSettings = _userSettingsFixture.Create();

        // Act
        Result<Updated> result = userSettings.UpdateSettings(isPaginationEnabled: true, itemsPerPage: itemsPerPage, shouldIgnoreThePrefixForAlphaPicker: false, isThemeCachingEnabled: true, shouldAggregateMetadataWhenMissing: false, shouldRenderPdfAsImages: false, shouldPreserveBookStyles: false);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero, result.FirstError);
    }
}
