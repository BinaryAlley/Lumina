#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Commands.CompleteConsumption;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.CompleteConsumption;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.CompleteConsumption;

/// <summary>
/// Contains unit tests for the <see cref="CompleteConsumptionCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionCommandValidatorTests
{
    private readonly CompleteConsumptionCommandValidator _validator = new();
    private readonly CompleteConsumptionCommandFixture _completeConsumptionCommandFixture = new();

    [Fact]
    public void Validate_WhenConsumptionRecordIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(includeConsumptionRecordId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // the consumption record Id is empty
    [InlineData("   ")] // the consumption record Id is whitespace only
    [InlineData("not-a-guid")] // the consumption record Id is not a Guid
    [InlineData("00000000-0000-0000-0000-000000000000")] // the consumption record Id is the empty Guid
    public void Validate_WhenConsumptionRecordIdIsEmptyOrInvalid_ShouldHaveValidationError(string consumptionRecordId)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consumptionRecordId: consumptionRecordId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenConsumptionRecordIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPositionKindIsNullAndNoPositionFieldsAreSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            includePositionKind: false,
            includePositionTimeOffsetInSeconds: false,
            includePositionLocation: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsNullAndTimeOffsetIsSupplied_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            includePositionKind: false,
            positionTimeOffsetInSeconds: 60);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsNullAndLocationIsSupplied_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            includePositionKind: false,
            positionLocation: "chapter-1");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsUnknown_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(positionKind: "Sideways");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsUndefinedNumericValue_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(positionKind: "99");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsTimeOffsetAndOffsetIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: 1800);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsTimeOffsetAndOffsetIsMissing_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.TimeOffset),
            includePositionTimeOffsetInSeconds: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsTimeOffsetAndLocationIsSupplied_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: 1800,
            positionLocation: "chapter-1");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Theory]
    [InlineData(-1)] // a negative offset cannot describe a position within a media item
    [InlineData(-0.5)] // a fractional negative offset is rejected the same way
    public void Validate_WhenPositionKindIsTimeOffsetAndOffsetIsNegative_ShouldHaveValidationError(double timeOffsetInSeconds)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: timeOffsetInSeconds);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Theory]
    [InlineData(double.NaN)] // not a number is not a position
    [InlineData(double.PositiveInfinity)] // an infinite offset is not a position
    [InlineData(double.NegativeInfinity)] // a negative infinite offset is not a position either
    public void Validate_WhenPositionKindIsTimeOffsetAndOffsetIsNotFinite_ShouldHaveValidationError(double timeOffsetInSeconds)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: timeOffsetInSeconds);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsTimeOffsetAndOffsetExceedsTheMaximum_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: 2_678_401);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsTimeOffsetAndOffsetIsExactlyTheMaximum_ShouldNotHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: 2_678_400);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsLocationAndLocationIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.Location),
            includePositionTimeOffsetInSeconds: false,
            positionLocation: "chapter-1");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsLocationAndLocationIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.Location),
            includePositionTimeOffsetInSeconds: false,
            positionLocation: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsLocationAndLocationIsAtTheMaximumLength_ShouldNotHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.Location),
            includePositionTimeOffsetInSeconds: false,
            positionLocation: new string('a', 4096));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsLocationAndLocationExceedsTheMaximumLength_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.Location),
            includePositionTimeOffsetInSeconds: false,
            positionLocation: new string('a', 4097));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenPositionKindIsLocationAndTimeOffsetIsSupplied_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            positionKind: nameof(MediaPositionKind.Location),
            positionTimeOffsetInSeconds: 1800,
            positionLocation: "chapter-1");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidMediaPosition);
    }

    [Fact]
    public void Validate_WhenProgressFractionIsMissing_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(includeProgressFraction: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidProgressFraction);
    }

    [Theory]
    [InlineData(-0.1)] // a fraction below zero is not a fraction of the media item
    [InlineData(1.1)] // a fraction above one is not a fraction of the media item
    public void Validate_WhenProgressFractionIsOutsideItsRange_ShouldHaveValidationError(double progressFraction)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(progressFraction: progressFraction);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidProgressFraction);
    }

    [Theory]
    [InlineData(double.NaN)] // not a number is not a fraction
    [InlineData(double.PositiveInfinity)] // an infinite fraction is not a fraction
    [InlineData(double.NegativeInfinity)] // a negative infinite fraction is not a fraction
    public void Validate_WhenProgressFractionIsNotFinite_ShouldHaveValidationError(double progressFraction)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(progressFraction: progressFraction);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.InvalidProgressFraction);
    }

    [Theory]
    [InlineData(0)] // the start of the media item is a valid fraction
    [InlineData(0.5)] // a fraction in between the boundaries is valid
    [InlineData(1)] // the end of the media item is a valid fraction
    public void Validate_WhenProgressFractionIsWithinItsRange_ShouldNotHaveValidationError(double progressFraction)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(progressFraction: progressFraction);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.InvalidProgressFraction);
    }

    [Fact]
    public void Validate_WhenConsumedDurationIsMissing_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(includeConsumedDurationInSeconds: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumedDurationMustBeNonNegative);
    }

    [Theory]
    [InlineData(-1)] // a negative duration cannot describe consumed time
    [InlineData(-0.5)] // a fractional negative duration is rejected the same way
    public void Validate_WhenConsumedDurationIsNegative_ShouldHaveValidationError(double consumedDurationInSeconds)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consumedDurationInSeconds: consumedDurationInSeconds);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumedDurationMustBeNonNegative);
    }

    [Theory]
    [InlineData(double.NaN)] // not a number is not a duration
    [InlineData(double.PositiveInfinity)] // an infinite duration is not a duration
    [InlineData(double.NegativeInfinity)] // a negative infinite duration is not a duration
    public void Validate_WhenConsumedDurationIsNotFinite_ShouldHaveValidationError(double consumedDurationInSeconds)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consumedDurationInSeconds: consumedDurationInSeconds);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumedDurationMustBeNonNegative);
    }

    [Fact]
    public void Validate_WhenConsumedDurationExceedsTheMaximum_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consumedDurationInSeconds: 2_678_401);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumedDurationIsOutOfRange);
    }

    [Theory]
    [InlineData(0)] // a session that consumed no time is valid
    [InlineData(2_678_400)] // exactly the maximum accepted duration is valid
    public void Validate_WhenConsumedDurationIsWithinItsRange_ShouldNotHaveValidationError(double consumedDurationInSeconds)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consumedDurationInSeconds: consumedDurationInSeconds);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumedDurationMustBeNonNegative);
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumedDurationIsOutOfRange);
    }

    [Fact]
    public void Validate_WhenConsecutivePagesReadIsMissing_ShouldNotHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(includeConsecutivePagesRead: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsecutivePagesReadMustBeNonNegative);
    }

    [Fact]
    public void Validate_WhenConsecutivePagesReadIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consecutivePagesRead: -1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsecutivePagesReadMustBeNonNegative);
    }

    [Theory]
    [InlineData(0)] // no pages read is still a valid count
    [InlineData(42)] // a positive count is valid
    public void Validate_WhenConsecutivePagesReadIsNotNegative_ShouldNotHaveValidationError(int consecutivePagesRead)
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consecutivePagesRead: consecutivePagesRead);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsecutivePagesReadMustBeNonNegative);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
