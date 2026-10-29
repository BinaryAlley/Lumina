#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserSettingsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsRequestMappingTests
{
    private readonly UpdateUserSettingsRequestFixture _updateUserSettingsRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: true,
            itemsPerPage: 48,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false,
            shouldRenderPdfAsImages: true,
            shouldPreserveBookStyles: true,
            shouldKeepDownloadingWhilePaused: false,
            shouldAutoplay: false,
            isGaplessPlaybackEnabled: false,
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 20,
            vuMeterSegmentGap: 4,
            vuMeterMediumThreshold: 0.35,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 1200,
            vuMeterReleasePerSecond: 7.5,
            vuMeterPeakReleasePerSecond: 0.25
        );

        // Act
        UpdateUserSettingsCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(request.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(request.ShouldIgnoreThePrefixForAlphaPicker, result.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(request.IsThemeCachingEnabled, result.IsThemeCachingEnabled);
        Assert.Equal(request.ShouldRenderPdfAsImages, result.ShouldRenderPdfAsImages);
        Assert.Equal(request.ShouldPreserveBookStyles, result.ShouldPreserveBookStyles);
        Assert.Equal(request.ShouldKeepDownloadingWhilePaused, result.ShouldKeepDownloadingWhilePaused);
        Assert.False(result.ShouldKeepDownloadingWhilePaused);
        Assert.Equal(request.ShouldAutoplay, result.ShouldAutoplay);
        Assert.False(result.ShouldAutoplay);
        Assert.Equal(request.IsGaplessPlaybackEnabled, result.IsGaplessPlaybackEnabled);
        Assert.False(result.IsGaplessPlaybackEnabled);
        Assert.Equal(VuMeterOrientation.Horizontal, result.VuMeterOrientation);
        Assert.Equal(20, result.VuMeterSegmentCount);
        Assert.Equal(4, result.VuMeterSegmentGap);
        Assert.Equal(0.35, result.VuMeterMediumThreshold);
        Assert.Equal(0.9, result.VuMeterHighThreshold);
        Assert.False(result.VuMeterShowPeak);
        Assert.Equal(1200, result.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(7.5, result.VuMeterReleasePerSecond);
        Assert.Equal(0.25, result.VuMeterPeakReleasePerSecond);
    }
}
