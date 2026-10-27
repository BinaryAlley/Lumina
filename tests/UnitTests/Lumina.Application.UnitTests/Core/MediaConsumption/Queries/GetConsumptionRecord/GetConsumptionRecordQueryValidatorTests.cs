#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionRecord;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetConsumptionRecord;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetConsumptionRecord;

/// <summary>
/// Contains unit tests for the <see cref="GetConsumptionRecordQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionRecordQueryValidatorTests
{
    private readonly GetConsumptionRecordQueryValidator _validator = new();
    private readonly GetConsumptionRecordQueryFixture _getConsumptionRecordQueryFixture = new();

    [Fact]
    public void Validate_WhenConsumptionRecordIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create(includeConsumptionRecordId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

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
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create(consumptionRecordId: consumptionRecordId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenConsumptionRecordIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
