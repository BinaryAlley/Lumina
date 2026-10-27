#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionRecord;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.DeleteConsumptionRecord;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.DeleteConsumptionRecord;

/// <summary>
/// Contains unit tests for the <see cref="DeleteConsumptionRecordCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionRecordCommandValidatorTests
{
    private readonly DeleteConsumptionRecordCommandValidator _validator = new();
    private readonly DeleteConsumptionRecordCommandFixture _deleteConsumptionRecordCommandFixture = new();

    [Fact]
    public void Validate_WhenConsumptionRecordIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create(includeConsumptionRecordId: false);

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
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create(consumptionRecordId: consumptionRecordId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenConsumptionRecordIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTargetUserIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create(includeTargetUserId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Theory]
    [InlineData("")] // the target user Id is empty
    [InlineData("   ")] // the target user Id is whitespace only
    [InlineData("not-a-guid")] // the target user Id is not a Guid
    [InlineData("00000000-0000-0000-0000-000000000000")] // the target user Id is the empty Guid
    public void Validate_WhenTargetUserIdIsEmptyOrInvalid_ShouldHaveValidationError(string targetUserId)
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create(targetUserId: targetUserId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetUserIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
