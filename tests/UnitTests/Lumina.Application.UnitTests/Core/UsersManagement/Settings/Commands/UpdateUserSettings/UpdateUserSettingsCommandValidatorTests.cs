#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Application.Fixtures.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserSettingsCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsCommandValidatorTests
{
    private readonly UpdateUserSettingsCommandValidator _validator = new();
    private readonly UpdateUserSettingsCommandFixture _updateUserSettingsCommandFixture = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WhenItemsPerPageIsNotPositive_ShouldHaveValidationError(int itemsPerPage)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(itemsPerPage: itemsPerPage);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(48)]
    [InlineData(1000000)]
    public void Validate_WhenItemsPerPageIsPositive_ShouldNotHaveValidationError(int itemsPerPage)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(itemsPerPage: itemsPerPage);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenVuMeterOrientationIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterOrientation: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterOrientationCannotBeNull);
    }

    [Fact]
    public void Validate_WhenVuMeterSegmentCountIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterSegmentCount: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterSegmentCountCannotBeNull);
    }

    [Fact]
    public void Validate_WhenVuMeterSegmentGapIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterSegmentGap: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterSegmentGapCannotBeNull);
    }

    [Fact]
    public void Validate_WhenVuMeterMediumThresholdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterMediumThreshold: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterMediumThresholdCannotBeNull);
    }

    [Fact]
    public void Validate_WhenVuMeterHighThresholdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterHighThreshold: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterHighThresholdCannotBeNull);
    }

    [Fact]
    public void Validate_WhenVuMeterShowPeakIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterShowPeak: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterShowPeakCannotBeNull);
    }

    [Fact]
    public void Validate_WhenVuMeterPeakHoldDurationIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterPeakHoldDurationInMilliseconds: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterPeakHoldDurationCannotBeNull);
    }

    [Fact]
    public void Validate_WhenVuMeterReleasePerSecondIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterReleasePerSecond: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterReleasePerSecondCannotBeNull);
    }

    [Fact]
    public void Validate_WhenVuMeterPeakReleasePerSecondIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(includeVuMeterPeakReleasePerSecond: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterPeakReleasePerSecondCannotBeNull);
    }

    [Theory]
    [InlineData(999)] // a value that is not a defined member of the enumeration
    [InlineData(-1)] // a negative value that is not a defined member of the enumeration
    public void Validate_WhenVuMeterOrientationIsNotDefined_ShouldHaveValidationError(int vuMeterOrientationValue)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterOrientation: (VuMeterOrientation)vuMeterOrientationValue);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.InvalidVuMeterOrientation);
    }

    [Theory]
    [InlineData(VuMeterOrientation.Vertical)]
    [InlineData(VuMeterOrientation.Horizontal)]
    public void Validate_WhenVuMeterOrientationIsDefined_ShouldNotHaveValidationError(VuMeterOrientation vuMeterOrientation)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(vuMeterOrientation: vuMeterOrientation);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.InvalidVuMeterOrientation);
    }

    [Theory]
    [InlineData(0)] // below the minimum
    [InlineData(-1)] // negative
    [InlineData(129)] // above the maximum
    [InlineData(int.MaxValue)] // far above the maximum
    public void Validate_WhenVuMeterSegmentCountIsOutOfRange_ShouldHaveValidationError(int vuMeterSegmentCount)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(vuMeterSegmentCount: vuMeterSegmentCount);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterSegmentCountMustBeBetweenOneAndOneHundredTwentyEight);
    }

    [Theory]
    [InlineData(1)] // the minimum
    [InlineData(64)] // a middle value
    [InlineData(128)] // the maximum
    public void Validate_WhenVuMeterSegmentCountIsInRange_ShouldNotHaveValidationError(int vuMeterSegmentCount)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(vuMeterSegmentCount: vuMeterSegmentCount);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterSegmentCountMustBeBetweenOneAndOneHundredTwentyEight);
    }

    [Theory]
    [InlineData(-1)] // below the minimum
    [InlineData(33)] // above the maximum
    [InlineData(int.MinValue)] // far below the minimum
    public void Validate_WhenVuMeterSegmentGapIsOutOfRange_ShouldHaveValidationError(int vuMeterSegmentGap)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(vuMeterSegmentGap: vuMeterSegmentGap);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterSegmentGapMustBeBetweenZeroAndThirtyTwo);
    }

    [Theory]
    [InlineData(0)] // the minimum
    [InlineData(16)] // a middle value
    [InlineData(32)] // the maximum
    public void Validate_WhenVuMeterSegmentGapIsInRange_ShouldNotHaveValidationError(int vuMeterSegmentGap)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(vuMeterSegmentGap: vuMeterSegmentGap);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterSegmentGapMustBeBetweenZeroAndThirtyTwo);
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(1.01)] // just above the maximum
    [InlineData(double.MinValue)] // far below the minimum
    public void Validate_WhenVuMeterMediumThresholdIsOutOfRange_ShouldHaveValidationError(double vuMeterMediumThreshold)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterMediumThreshold: vuMeterMediumThreshold,
            vuMeterHighThreshold: 1.0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterMediumThresholdMustBeBetweenZeroAndOne);
    }

    [Theory]
    [InlineData(0.0)] // the minimum
    [InlineData(0.5)] // a middle value
    [InlineData(1.0)] // the maximum
    public void Validate_WhenVuMeterMediumThresholdIsInRange_ShouldNotHaveValidationError(double vuMeterMediumThreshold)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterMediumThreshold: vuMeterMediumThreshold,
            vuMeterHighThreshold: 1.0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterMediumThresholdMustBeBetweenZeroAndOne);
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(1.01)] // just above the maximum
    [InlineData(double.MaxValue)] // far above the maximum
    public void Validate_WhenVuMeterHighThresholdIsOutOfRange_ShouldHaveValidationError(double vuMeterHighThreshold)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterMediumThreshold: 0.0,
            vuMeterHighThreshold: vuMeterHighThreshold);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterHighThresholdMustBeBetweenZeroAndOne);
    }

    [Fact]
    public void Validate_WhenVuMeterHighThresholdIsAtTheMinimum_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterMediumThreshold: 0.0,
            vuMeterHighThreshold: 0.0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterHighThresholdMustBeBetweenZeroAndOne);
    }

    [Fact]
    public void Validate_WhenVuMeterHighThresholdIsAtTheMaximum_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterMediumThreshold: 0.5,
            vuMeterHighThreshold: 1.0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterHighThresholdMustBeBetweenZeroAndOne);
    }

    [Fact]
    public void Validate_WhenVuMeterHighThresholdIsLowerThanMediumThreshold_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterMediumThreshold: 0.9,
            vuMeterHighThreshold: 0.1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterHighThresholdMustBeGreaterThanOrEqualToMediumThreshold);
    }

    [Theory]
    [InlineData(0.5, 0.5)] // the thresholds are equal
    [InlineData(0.4, 0.9)] // the high threshold is greater than the medium one
    public void Validate_WhenVuMeterHighThresholdIsGreaterThanOrEqualToMediumThreshold_ShouldNotHaveValidationError(
        double vuMeterMediumThreshold,
        double vuMeterHighThreshold)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterMediumThreshold: vuMeterMediumThreshold,
            vuMeterHighThreshold: vuMeterHighThreshold);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterHighThresholdMustBeGreaterThanOrEqualToMediumThreshold);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_WhenVuMeterShowPeakIsSpecified_ShouldNotHaveValidationError(bool vuMeterShowPeak)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(vuMeterShowPeak: vuMeterShowPeak);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterShowPeakCannotBeNull);
    }

    [Theory]
    [InlineData(-1)] // below the minimum
    [InlineData(10001)] // above the maximum
    [InlineData(int.MinValue)] // far below the minimum
    public void Validate_WhenVuMeterPeakHoldDurationIsOutOfRange_ShouldHaveValidationError(int vuMeterPeakHoldDurationInMilliseconds)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterPeakHoldDurationInMilliseconds: vuMeterPeakHoldDurationInMilliseconds);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterPeakHoldDurationMustBeBetweenZeroAndTenThousandMilliseconds);
    }

    [Theory]
    [InlineData(0)] // the minimum
    [InlineData(650)] // a middle value
    [InlineData(10000)] // the maximum
    public void Validate_WhenVuMeterPeakHoldDurationIsInRange_ShouldNotHaveValidationError(int vuMeterPeakHoldDurationInMilliseconds)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterPeakHoldDurationInMilliseconds: vuMeterPeakHoldDurationInMilliseconds);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterPeakHoldDurationMustBeBetweenZeroAndTenThousandMilliseconds);
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(100.01)] // just above the maximum
    [InlineData(double.MinValue)] // far below the minimum
    public void Validate_WhenVuMeterReleasePerSecondIsOutOfRange_ShouldHaveValidationError(double vuMeterReleasePerSecond)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterReleasePerSecond: vuMeterReleasePerSecond);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterReleasePerSecondMustBeBetweenZeroAndOneHundred);
    }

    [Theory]
    [InlineData(0.0)] // the minimum
    [InlineData(3.0)] // a middle value
    [InlineData(100.0)] // the maximum
    public void Validate_WhenVuMeterReleasePerSecondIsInRange_ShouldNotHaveValidationError(double vuMeterReleasePerSecond)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterReleasePerSecond: vuMeterReleasePerSecond);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterReleasePerSecondMustBeBetweenZeroAndOneHundred);
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(100.01)] // just above the maximum
    [InlineData(double.MaxValue)] // far above the maximum
    public void Validate_WhenVuMeterPeakReleasePerSecondIsOutOfRange_ShouldHaveValidationError(double vuMeterPeakReleasePerSecond)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterPeakReleasePerSecond: vuMeterPeakReleasePerSecond);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VuMeterPeakReleasePerSecondMustBeBetweenZeroAndOneHundred);
    }

    [Theory]
    [InlineData(0.0)] // the minimum
    [InlineData(0.65)] // a middle value
    [InlineData(100.0)] // the maximum
    public void Validate_WhenVuMeterPeakReleasePerSecondIsInRange_ShouldNotHaveValidationError(double vuMeterPeakReleasePerSecond)
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create(
            vuMeterPeakReleasePerSecond: vuMeterPeakReleasePerSecond);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VuMeterPeakReleasePerSecondMustBeBetweenZeroAndOneHundred);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        UpdateUserSettingsCommand command = _updateUserSettingsCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
