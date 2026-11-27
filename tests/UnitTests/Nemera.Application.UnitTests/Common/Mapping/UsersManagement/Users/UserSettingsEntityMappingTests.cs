#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserSettingsEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsEntityMappingTests
{
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingValidRepositoryEntity_ShouldMapCorrectly()
    {
        // Arrange
        UserSettingsEntity entity = _userSettingsEntityFixture.Create(
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 24,
            vuMeterSegmentGap: 2,
            vuMeterMediumThreshold: 0.4,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 900,
            vuMeterReleasePerSecond: 2.5,
            vuMeterPeakReleasePerSecond: 0.9);

        // Act
        Result<UserSettings> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(entity.Id, result.Value.Id.Value);
        Assert.Equal(entity.UserId, result.Value.UserId.Value);
        Assert.Equal(entity.IsPaginationEnabled, result.Value.IsPaginationEnabled);
        Assert.Equal(entity.ItemsPerPage, result.Value.ItemsPerPage);
        Assert.Equal(entity.ShouldIgnoreThePrefixForAlphaPicker, result.Value.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(entity.IsThemeCachingEnabled, result.Value.IsThemeCachingEnabled);
        Assert.Equal(entity.ShouldRenderPdfAsImages, result.Value.ShouldRenderPdfAsImages);
        Assert.Equal(entity.ShouldPreserveBookStyles, result.Value.ShouldPreserveBookStyles);
        Assert.Equal(entity.VuMeterOrientation, result.Value.VuMeterOrientation);
        Assert.Equal(entity.VuMeterSegmentCount, result.Value.VuMeterSegmentCount);
        Assert.Equal(entity.VuMeterSegmentGap, result.Value.VuMeterSegmentGap);
        Assert.Equal(entity.VuMeterMediumThreshold, result.Value.VuMeterMediumThreshold);
        Assert.Equal(entity.VuMeterHighThreshold, result.Value.VuMeterHighThreshold);
        Assert.Equal(entity.VuMeterShowPeak, result.Value.VuMeterShowPeak);
        Assert.Equal(entity.VuMeterPeakHoldDurationInMilliseconds, result.Value.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(entity.VuMeterReleasePerSecond, result.Value.VuMeterReleasePerSecond);
        Assert.Equal(entity.VuMeterPeakReleasePerSecond, result.Value.VuMeterPeakReleasePerSecond);
    }

    [Fact]
    public void ToResponse_WhenMappingValidRepositoryEntity_ShouldMapCorrectly()
    {
        // Arrange
        UserSettingsEntity entity = _userSettingsEntityFixture.Create(
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 24,
            vuMeterSegmentGap: 2,
            vuMeterMediumThreshold: 0.4,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 900,
            vuMeterReleasePerSecond: 2.5,
            vuMeterPeakReleasePerSecond: 0.9);

        // Act
        UserSettingsResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.UserId, result.UserId);
        Assert.Equal(entity.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(entity.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(entity.ShouldIgnoreThePrefixForAlphaPicker, result.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(entity.IsThemeCachingEnabled, result.IsThemeCachingEnabled);
        Assert.Equal(entity.ShouldRenderPdfAsImages, result.ShouldRenderPdfAsImages);
        Assert.Equal(entity.ShouldPreserveBookStyles, result.ShouldPreserveBookStyles);
        Assert.Equal(entity.VuMeterOrientation, result.VuMeterOrientation);
        Assert.Equal(entity.VuMeterSegmentCount, result.VuMeterSegmentCount);
        Assert.Equal(entity.VuMeterSegmentGap, result.VuMeterSegmentGap);
        Assert.Equal(entity.VuMeterMediumThreshold, result.VuMeterMediumThreshold);
        Assert.Equal(entity.VuMeterHighThreshold, result.VuMeterHighThreshold);
        Assert.Equal(entity.VuMeterShowPeak, result.VuMeterShowPeak);
        Assert.Equal(entity.VuMeterPeakHoldDurationInMilliseconds, result.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(entity.VuMeterReleasePerSecond, result.VuMeterReleasePerSecond);
        Assert.Equal(entity.VuMeterPeakReleasePerSecond, result.VuMeterPeakReleasePerSecond);
    }
}
