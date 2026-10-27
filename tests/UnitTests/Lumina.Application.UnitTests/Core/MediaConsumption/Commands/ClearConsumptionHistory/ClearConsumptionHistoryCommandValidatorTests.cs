#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Commands.ClearConsumptionHistory;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.ClearConsumptionHistory;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.ClearConsumptionHistory;

/// <summary>
/// Contains unit tests for the <see cref="ClearConsumptionHistoryCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearConsumptionHistoryCommandValidatorTests
{
    private readonly ClearConsumptionHistoryCommandValidator _validator = new();
    private readonly ClearConsumptionHistoryCommandFixture _clearConsumptionHistoryCommandFixture = new();

    [Fact]
    public void Validate_WhenTargetUserIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create(includeTargetUserId: false);

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
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create(targetUserId: targetUserId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetUserIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
