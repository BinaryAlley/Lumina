#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using Lumina.Application.Fixtures.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaybackStateCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateCommandValidatorTests
{
    private readonly UpdatePlaybackStateCommandValidator _validator = new();
    private readonly UpdatePlaybackStateCommandFixture _updatePlaybackStateCommandFixture = new();

    [Fact]
    public void Validate_WhenVolumeIsNull_ShouldHaveVolumeCannotBeNullErrorAndNoRangeError()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create(includeVolume: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VolumeCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.UserSettings.VolumeMustBeBetweenZeroAndOne);
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(-1.0)] // well below the minimum
    [InlineData(double.MinValue)] // far below the minimum
    public void Validate_WhenVolumeIsBelowZero_ShouldHaveRangeError(double volume)
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create(volume: volume);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VolumeMustBeBetweenZeroAndOne);
    }

    [Theory]
    [InlineData(1.01)] // just above the maximum
    [InlineData(2.0)] // well above the maximum
    [InlineData(double.MaxValue)] // far above the maximum
    public void Validate_WhenVolumeIsAboveOne_ShouldHaveRangeError(double volume)
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create(volume: volume);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.VolumeMustBeBetweenZeroAndOne);
    }

    [Theory]
    [InlineData(0.0)] // the minimum
    [InlineData(0.5)] // a middle value
    [InlineData(1.0)] // the maximum
    public void Validate_WhenVolumeIsInRange_ShouldNotHaveRangeError(double volume)
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create(volume: volume);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.VolumeMustBeBetweenZeroAndOne);
        result.ShouldNotHaveValidationError(Errors.UserSettings.VolumeCannotBeNull);
    }

    [Fact]
    public void Validate_WhenIsMutedIsNull_ShouldHaveIsMutedCannotBeNullError()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create(includeIsMuted: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.UserSettings.IsMutedCannotBeNull);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_WhenIsMutedIsSpecified_ShouldNotHaveIsMutedCannotBeNullError(bool isMuted)
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create(isMuted: isMuted);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.UserSettings.IsMutedCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        UpdatePlaybackStateCommand command = _updatePlaybackStateCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
