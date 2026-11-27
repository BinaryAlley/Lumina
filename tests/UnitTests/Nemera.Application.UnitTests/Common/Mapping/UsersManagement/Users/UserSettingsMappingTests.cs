#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserSettingsMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsMappingTests
{
    private readonly UserSettingsFixture _userSettingsFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingValidDomainEntity_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserSettings domainEntity = _userSettingsFixture.Create(
            userId: userId,
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
        UserSettingsEntity result = domainEntity.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainEntity.Id.Value, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(domainEntity.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(domainEntity.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(domainEntity.ShouldIgnoreThePrefixForAlphaPicker, result.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(domainEntity.ShouldRenderPdfAsImages, result.ShouldRenderPdfAsImages);
        Assert.Equal(domainEntity.ShouldPreserveBookStyles, result.ShouldPreserveBookStyles);
        Assert.Equal(domainEntity.VuMeterOrientation, result.VuMeterOrientation);
        Assert.Equal(domainEntity.VuMeterSegmentCount, result.VuMeterSegmentCount);
        Assert.Equal(domainEntity.VuMeterSegmentGap, result.VuMeterSegmentGap);
        Assert.Equal(domainEntity.VuMeterMediumThreshold, result.VuMeterMediumThreshold);
        Assert.Equal(domainEntity.VuMeterHighThreshold, result.VuMeterHighThreshold);
        Assert.Equal(domainEntity.VuMeterShowPeak, result.VuMeterShowPeak);
        Assert.Equal(domainEntity.VuMeterPeakHoldDurationInMilliseconds, result.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(domainEntity.VuMeterReleasePerSecond, result.VuMeterReleasePerSecond);
        Assert.Equal(domainEntity.VuMeterPeakReleasePerSecond, result.VuMeterPeakReleasePerSecond);
    }

    [Fact]
    public void ToResponse_WhenMappingValidDomainEntity_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserSettings domainEntity = _userSettingsFixture.Create(
            userId: userId,
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
        UserSettingsResponse result = domainEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(domainEntity.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(domainEntity.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(domainEntity.ShouldIgnoreThePrefixForAlphaPicker, result.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(domainEntity.ShouldRenderPdfAsImages, result.ShouldRenderPdfAsImages);
        Assert.Equal(domainEntity.ShouldPreserveBookStyles, result.ShouldPreserveBookStyles);
        Assert.Equal(domainEntity.VuMeterOrientation, result.VuMeterOrientation);
        Assert.Equal(domainEntity.VuMeterSegmentCount, result.VuMeterSegmentCount);
        Assert.Equal(domainEntity.VuMeterSegmentGap, result.VuMeterSegmentGap);
        Assert.Equal(domainEntity.VuMeterMediumThreshold, result.VuMeterMediumThreshold);
        Assert.Equal(domainEntity.VuMeterHighThreshold, result.VuMeterHighThreshold);
        Assert.Equal(domainEntity.VuMeterShowPeak, result.VuMeterShowPeak);
        Assert.Equal(domainEntity.VuMeterPeakHoldDurationInMilliseconds, result.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(domainEntity.VuMeterReleasePerSecond, result.VuMeterReleasePerSecond);
        Assert.Equal(domainEntity.VuMeterPeakReleasePerSecond, result.VuMeterPeakReleasePerSecond);
    }
}
