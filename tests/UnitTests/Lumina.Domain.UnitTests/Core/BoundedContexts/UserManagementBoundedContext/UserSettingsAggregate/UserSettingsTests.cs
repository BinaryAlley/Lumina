#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
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
        Assert.Equal(VuMeterOrientation.Vertical, result.Value.VuMeterOrientation);
        Assert.Equal(12, result.Value.VuMeterSegmentCount);
        Assert.Equal(1, result.Value.VuMeterSegmentGap);
        Assert.Equal(0.6, result.Value.VuMeterMediumThreshold);
        Assert.Equal(0.8, result.Value.VuMeterHighThreshold);
        Assert.True(result.Value.VuMeterShowPeak);
        Assert.Equal(650, result.Value.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(3.0, result.Value.VuMeterReleasePerSecond);
        Assert.Equal(0.65, result.Value.VuMeterPeakReleasePerSecond);
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
        Assert.Equal(VuMeterOrientation.Vertical, result.Value.VuMeterOrientation);
        Assert.Equal(12, result.Value.VuMeterSegmentCount);
        Assert.Equal(1, result.Value.VuMeterSegmentGap);
        Assert.Equal(0.6, result.Value.VuMeterMediumThreshold);
        Assert.Equal(0.8, result.Value.VuMeterHighThreshold);
        Assert.True(result.Value.VuMeterShowPeak);
        Assert.Equal(650, result.Value.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(3.0, result.Value.VuMeterReleasePerSecond);
        Assert.Equal(0.65, result.Value.VuMeterPeakReleasePerSecond);
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
        Result<UserSettings> result = UserSettings.Create(
            userId,
            isPaginationEnabled: false,
            itemsPerPage: 24,
            shouldIgnoreThePrefixForAlphaPicker: true,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: true,
            shouldAggregateArtworkWhenMissing: true,
            shouldRenderPdfAsImages: true,
            shouldPreserveBookStyles: true,
            shouldKeepDownloadingWhilePaused: true,
            shouldAutoplay: true,
            isGaplessPlaybackEnabled: true,
            replayGainMode: ReplayGainMode.Off,
            streamingQuality: PlaybackStreamingQuality.Auto,
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 20,
            vuMeterSegmentGap: 4,
            vuMeterMediumThreshold: 0.35,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 1200,
            vuMeterReleasePerSecond: 7.5,
            vuMeterPeakReleasePerSecond: 0.25,
            volume: 1.0,
            isMuted: false);

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
        Assert.Equal(VuMeterOrientation.Horizontal, result.Value.VuMeterOrientation);
        Assert.Equal(20, result.Value.VuMeterSegmentCount);
        Assert.Equal(4, result.Value.VuMeterSegmentGap);
        Assert.Equal(0.35, result.Value.VuMeterMediumThreshold);
        Assert.Equal(0.9, result.Value.VuMeterHighThreshold);
        Assert.False(result.Value.VuMeterShowPeak);
        Assert.Equal(1200, result.Value.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(7.5, result.Value.VuMeterReleasePerSecond);
        Assert.Equal(0.25, result.Value.VuMeterPeakReleasePerSecond);
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

    [Fact]
    public void Create_WhenCalledWithPreExistingIdAndValidValues_ShouldCreateSettingsWithThatId()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        UserSettingsId id = _userSettingsIdFixture.Create();

        // Act
        Result<UserSettings> result = UserSettings.Create(
            id,
            userId,
            isPaginationEnabled: true,
            itemsPerPage: 24,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false,
            shouldAggregateArtworkWhenMissing: false,
            shouldRenderPdfAsImages: false,
            shouldPreserveBookStyles: true,
            shouldKeepDownloadingWhilePaused: true,
            shouldAutoplay: true,
            isGaplessPlaybackEnabled: true,
            replayGainMode: ReplayGainMode.Off,
            streamingQuality: PlaybackStreamingQuality.Auto,
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 24,
            vuMeterSegmentGap: 3,
            vuMeterMediumThreshold: 0.4,
            vuMeterHighThreshold: 0.95,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 800,
            vuMeterReleasePerSecond: 9.5,
            vuMeterPeakReleasePerSecond: 0.5,
            volume: 1.0,
            isMuted: false);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(24, result.Value.ItemsPerPage);
        Assert.Equal(VuMeterOrientation.Horizontal, result.Value.VuMeterOrientation);
        Assert.Equal(24, result.Value.VuMeterSegmentCount);
        Assert.Equal(3, result.Value.VuMeterSegmentGap);
        Assert.Equal(0.4, result.Value.VuMeterMediumThreshold);
        Assert.Equal(0.95, result.Value.VuMeterHighThreshold);
        Assert.False(result.Value.VuMeterShowPeak);
        Assert.Equal(800, result.Value.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(9.5, result.Value.VuMeterReleasePerSecond);
        Assert.Equal(0.5, result.Value.VuMeterPeakReleasePerSecond);
    }

    [Fact]
    public void UpdateSettings_WhenCalledWithValidValues_ShouldUpdateSettings()
    {
        // Arrange
        UserSettings userSettings = _userSettingsFixture.Create();
        double previousVolume = userSettings.Volume;
        bool previousIsMuted = userSettings.IsMuted;

        // Act
        Result<Updated> result = userSettings.UpdateSettings(
            isPaginationEnabled: false,
            itemsPerPage: 12,
            shouldIgnoreThePrefixForAlphaPicker: true,
            isThemeCachingEnabled: false,
            shouldAggregateMetadataWhenMissing: true,
            shouldAggregateArtworkWhenMissing: true,
            shouldRenderPdfAsImages: true,
            shouldPreserveBookStyles: true,
            shouldKeepDownloadingWhilePaused: true,
            shouldAutoplay: true,
            isGaplessPlaybackEnabled: true,
            replayGainMode: ReplayGainMode.Off,
            streamingQuality: PlaybackStreamingQuality.Auto,
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 30,
            vuMeterSegmentGap: 5,
            vuMeterMediumThreshold: 0.3,
            vuMeterHighThreshold: 0.95,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 1500,
            vuMeterReleasePerSecond: 8.0,
            vuMeterPeakReleasePerSecond: 0.4);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(userSettings.IsPaginationEnabled);
        Assert.Equal(12, userSettings.ItemsPerPage);
        Assert.True(userSettings.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.False(userSettings.IsThemeCachingEnabled);
        Assert.True(userSettings.ShouldAggregateMetadataWhenMissing);
        Assert.True(userSettings.ShouldRenderPdfAsImages);
        Assert.True(userSettings.ShouldPreserveBookStyles);
        Assert.Equal(VuMeterOrientation.Horizontal, userSettings.VuMeterOrientation);
        Assert.Equal(30, userSettings.VuMeterSegmentCount);
        Assert.Equal(5, userSettings.VuMeterSegmentGap);
        Assert.Equal(0.3, userSettings.VuMeterMediumThreshold);
        Assert.Equal(0.95, userSettings.VuMeterHighThreshold);
        Assert.False(userSettings.VuMeterShowPeak);
        Assert.Equal(1500, userSettings.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(8.0, userSettings.VuMeterReleasePerSecond);
        Assert.Equal(0.4, userSettings.VuMeterPeakReleasePerSecond);
        // The playback state the player reports automatically is not part of the settings the user edits.
        Assert.Equal(previousVolume, userSettings.Volume);
        Assert.Equal(previousIsMuted, userSettings.IsMuted);
    }
}
