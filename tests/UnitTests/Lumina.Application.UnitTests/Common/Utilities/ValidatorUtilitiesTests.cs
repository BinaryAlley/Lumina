#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Application.Fixtures.Common.Utilities;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="ValidatorUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatorUtilitiesTests
{
    private readonly ValidatorUtilitiesTestValidator _validator = new();
    private readonly ValidatorUtilitiesTestRequestFixture _validatorUtilitiesTestRequestFixture = new();

    [Fact]
    public void Validate_WhenRequestIsFullyValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(4)] // value is greater than the comparison value
    [InlineData(100)] // value is much greater than the comparison value
    public void Validate_WhenIntValueIsGreaterThanComparison_ShouldNotHaveValidationError(int value)
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { IntValue = value };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_intValueError);
    }

    [Theory]
    [InlineData(3)] // value equals the comparison value
    [InlineData(1)] // value is smaller than the comparison value
    [InlineData(-5)] // value is negative and smaller than the comparison value
    public void Validate_WhenIntValueIsNotGreaterThanComparison_ShouldHaveValidationError(int value)
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { IntValue = value };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_intValueError);
    }

    [Fact]
    public void Validate_WhenNullableIntValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableIntValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_nullableIntValueError);
    }

    [Fact]
    public void Validate_WhenNullableIntValueIsNotGreaterThanComparison_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableIntValue = 2 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_nullableIntValueError);
    }

    [Fact]
    public void Validate_WhenDecimalValueIsNotGreaterThanComparison_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { DecimalValue = 3m };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_decimalValueError);
    }

    [Fact]
    public void Validate_WhenNullableDecimalValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableDecimalValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_nullableDecimalValueError);
    }

    [Fact]
    public void Validate_WhenNullableDecimalValueIsNotGreaterThanComparison_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableDecimalValue = 2m };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_nullableDecimalValueError);
    }

    [Fact]
    public void Validate_WhenTimeSpanValueIsNotGreaterThanComparison_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { TimeSpanValue = TimeSpan.FromHours(3) };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_timeSpanValueError);
    }

    [Fact]
    public void Validate_WhenIntValue2IsEqual_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { IntValue2 = 5 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_intValue2Error);
    }

    [Fact]
    public void Validate_WhenIntValue2IsSmaller_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { IntValue2 = 4 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_intValue2Error);
    }

    [Fact]
    public void Validate_WhenNullableIntValue2IsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableIntValue2 = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_nullableIntValue2Error);
    }

    [Fact]
    public void Validate_WhenNullableIntValue2IsSmaller_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableIntValue2 = 4 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_nullableIntValue2Error);
    }

    [Fact]
    public void Validate_WhenNullableDecimalValue2IsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableDecimalValue2 = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_nullableDecimalValue2Error);
    }

    [Fact]
    public void Validate_WhenNullableDecimalValue2IsSmaller_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableDecimalValue2 = 4m };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_nullableDecimalValue2Error);
    }

    [Fact]
    public void Validate_WhenNotNullValueIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NotNullValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_notNullValueError);
    }

    [Fact]
    public void Validate_WhenNotNullValueIsNotNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_notNullValueError);
    }

    [Theory]
    [InlineData(null)] // null string
    [InlineData("")] // empty string
    [InlineData("   ")] // white-space only string
    public void Validate_WhenNotEmptyValueIsEmpty_ShouldHaveValidationError(string? value)
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NotEmptyValue = value };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_notEmptyValueError);
    }

    [Fact]
    public void Validate_WhenNotEmptyValueIsNotBlank_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NotEmptyValue = "value" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_notEmptyValueError);
    }

    [Fact]
    public void Validate_WhenNotEmptyGuidValueIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NotEmptyGuidValue = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_notEmptyGuidValueError);
    }

    [Fact]
    public void Validate_WhenNotEmptyCollectionValueIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NotEmptyCollectionValue = [] };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_notEmptyCollectionValueError);
    }

    [Fact]
    public void Validate_WhenNotEmptyCollectionValueIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NotEmptyCollectionValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_notEmptyCollectionValueError);
    }

    [Fact]
    public void Validate_WhenNotEmptyCollectionValueHasItems_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NotEmptyCollectionValue = ["item"] };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_notEmptyCollectionValueError);
    }

    [Fact]
    public void Validate_WhenEnumValueIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { EnumValue = (ValidatorUtilitiesTestEnum)99 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_enumValueError);
    }

    [Fact]
    public void Validate_WhenEnumValueIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { EnumValue = ValidatorUtilitiesTestEnum.First };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_enumValueError);
    }

    [Fact]
    public void Validate_WhenNullableEnumValueIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableEnumValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_nullableEnumValueError);
    }

    [Fact]
    public void Validate_WhenNullableEnumValueIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableEnumValue = (ValidatorUtilitiesTestEnum)99 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_nullableEnumValueError);
    }

    [Fact]
    public void Validate_WhenNullableEnumValueIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { NullableEnumValue = ValidatorUtilitiesTestEnum.First };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_nullableEnumValueError);
    }

    [Fact]
    public void Validate_WhenMaxLengthValueExceedsMaximumLength_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MaxLengthValue = "abcdef" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_maxLengthValueError);
    }

    [Fact]
    public void Validate_WhenMaxLengthValueIsAtMaximumLength_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MaxLengthValue = "abcde" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_maxLengthValueError);
    }

    [Fact]
    public void Validate_WhenMaxLengthValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MaxLengthValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_maxLengthValueError);
    }

    [Fact]
    public void Validate_WhenMinLengthValueIsBelowMinimumLength_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MinLengthValue = "ab" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_minLengthValueError);
    }

    [Fact]
    public void Validate_WhenMinLengthValueIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MinLengthValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_minLengthValueError);
    }

    [Fact]
    public void Validate_WhenMinLengthValueIsAtMinimumLength_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MinLengthValue = "abc" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_minLengthValueError);
    }

    [Fact]
    public void Validate_WhenExactLengthValueDoesNotMatchExactLength_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { ExactLengthValue = "ab" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_exactLengthValueError);
    }

    [Fact]
    public void Validate_WhenExactLengthValueIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { ExactLengthValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_exactLengthValueError);
    }

    [Fact]
    public void Validate_WhenExactLengthValueMatchesExactLength_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { ExactLengthValue = "abc" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_exactLengthValueError);
    }

    [Fact]
    public void Validate_WhenRangeLengthValueIsOutsideRange_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { RangeLengthValue = "abcdef" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_rangeLengthValueError);
    }

    [Fact]
    public void Validate_WhenRangeLengthValueIsNull_ShouldHaveValidationErrorForRange()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { RangeLengthValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_rangeLengthValueError);
    }

    [Fact]
    public void Validate_WhenRangeLengthValueIsWithinRange_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { RangeLengthValue = "abcd" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_rangeLengthValueError);
    }

    [Fact]
    public void Validate_WhenMatchValueDoesNotMatchPattern_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MatchValue = "not-a-match" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_matchValueError);
    }

    [Fact]
    public void Validate_WhenMatchValueIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MatchValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_matchValueError);
    }

    [Fact]
    public void Validate_WhenMatchValueMatchesPattern_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { MatchValue = "abc-123" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_matchValueError);
    }

    [Fact]
    public void Validate_WhenSelectorMatchValueDoesNotMatchSelectedPattern_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { SelectorMatchValue = "invalid" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_selectorMatchValueError);
    }

    [Fact]
    public void Validate_WhenSelectorMatchValueMatchesSelectedPattern_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { SelectorMatchValue = "xyz-999" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_selectorMatchValueError);
    }

    [Fact]
    public void Validate_WhenInclusiveIntValueIsBelowMinimum_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { InclusiveIntValue = 0 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_inclusiveIntValueError);
    }

    [Fact]
    public void Validate_WhenInclusiveIntValueIsAboveMaximum_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { InclusiveIntValue = 11 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_inclusiveIntValueError);
    }

    [Fact]
    public void Validate_WhenInclusiveIntValueIsWithinRange_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { InclusiveIntValue = 1 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_inclusiveIntValueError);
    }

    [Fact]
    public void Validate_WhenInclusiveNullableIntValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { InclusiveNullableIntValue = null };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_inclusiveNullableIntValueError);
    }

    [Fact]
    public void Validate_WhenInclusiveNullableIntValueIsOutsideRange_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { InclusiveNullableIntValue = 12 };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_inclusiveNullableIntValueError);
    }

    [Fact]
    public void Validate_WhenEqualValueDiffersFromComparison_ShouldHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();
        request = request with { EqualValue = "different" };

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationError(s_equalValueError);
    }

    [Fact]
    public void Validate_WhenEqualValueEqualsComparison_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatorUtilitiesTestRequest request = _validatorUtilitiesTestRequestFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationError(s_equalValueError);
    }

    [ExcludeFromCodeCoverage]
    private sealed class ValidatorUtilitiesTestValidator : AbstractValidator<ValidatorUtilitiesTestRequest>
    {
        public ValidatorUtilitiesTestValidator()
        {
            RuleFor(request => request.IntValue).GreaterThan(3).WithError(s_intValueError);
            RuleFor(request => request.NullableIntValue).GreaterThan(3).WithError(s_nullableIntValueError);
            RuleFor(request => request.DecimalValue).GreaterThan(3m).WithError(s_decimalValueError);
            RuleFor(request => request.NullableDecimalValue).GreaterThan(3m).WithError(s_nullableDecimalValueError);
            RuleFor(request => request.TimeSpanValue).GreaterThan(TimeSpan.FromHours(3)).WithError(s_timeSpanValueError);
            RuleFor(request => request.IntValue2).GreaterThanOrEqualTo(5).WithError(s_intValue2Error);
            RuleFor(request => request.NullableIntValue2).GreaterThanOrEqualTo(5).WithError(s_nullableIntValue2Error);
            RuleFor(request => request.NullableDecimalValue2).GreaterThanOrEqualTo(5m).WithError(s_nullableDecimalValue2Error);
            RuleFor(request => request.NotNullValue).NotNull().WithError(s_notNullValueError);
            RuleFor(request => request.NotEmptyValue).NotEmpty().WithError(s_notEmptyValueError);
            RuleFor(request => request.NotEmptyGuidValue).NotEmpty().WithError(s_notEmptyGuidValueError);
            RuleFor(request => request.NotEmptyCollectionValue).NotEmpty().WithError(s_notEmptyCollectionValueError);
            RuleFor(request => request.EnumValue).IsInEnum().WithError(s_enumValueError);
            RuleFor(request => request.NullableEnumValue).IsInEnum().WithError(s_nullableEnumValueError);
            RuleFor(request => request.MaxLengthValue).MaximumLength(5).WithError(s_maxLengthValueError);
            RuleFor(request => request.MinLengthValue).MinimumLength(3).WithError(s_minLengthValueError);
            RuleFor(request => request.ExactLengthValue).Length(3).WithError(s_exactLengthValueError);
            RuleFor(request => request.RangeLengthValue).Length(3, 5).WithError(s_rangeLengthValueError);
            RuleFor(request => request.MatchValue).Matches("^[a-z]{3}-\\d{3}$").WithError(s_matchValueError);
            RuleFor(request => request.SelectorMatchValue).Matches(instance => instance.PatternValue).WithError(s_selectorMatchValueError);
            RuleFor(request => request.InclusiveIntValue).InclusiveBetween(1, 10).WithError(s_inclusiveIntValueError);
            RuleFor(request => request.InclusiveNullableIntValue).InclusiveBetween(1, 10).WithError(s_inclusiveNullableIntValueError);
            RuleFor(request => request.EqualValue).Equal(instance => instance.EqualComparisonValue).WithError(s_equalValueError);
        }
    }

    private static readonly Error s_intValueError = Error.Validation(description: "IntValueError");
    private static readonly Error s_nullableIntValueError = Error.Validation(description: "NullableIntValueError");
    private static readonly Error s_decimalValueError = Error.Validation(description: "DecimalValueError");
    private static readonly Error s_nullableDecimalValueError = Error.Validation(description: "NullableDecimalValueError");
    private static readonly Error s_timeSpanValueError = Error.Validation(description: "TimeSpanValueError");
    private static readonly Error s_intValue2Error = Error.Validation(description: "IntValue2Error");
    private static readonly Error s_nullableIntValue2Error = Error.Validation(description: "NullableIntValue2Error");
    private static readonly Error s_nullableDecimalValue2Error = Error.Validation(description: "NullableDecimalValue2Error");
    private static readonly Error s_notNullValueError = Error.Validation(description: "NotNullValueError");
    private static readonly Error s_notEmptyValueError = Error.Validation(description: "NotEmptyValueError");
    private static readonly Error s_notEmptyGuidValueError = Error.Validation(description: "NotEmptyGuidValueError");
    private static readonly Error s_notEmptyCollectionValueError = Error.Validation(description: "NotEmptyCollectionValueError");
    private static readonly Error s_enumValueError = Error.Validation(description: "EnumValueError");
    private static readonly Error s_nullableEnumValueError = Error.Validation(description: "NullableEnumValueError");
    private static readonly Error s_maxLengthValueError = Error.Validation(description: "MaxLengthValueError");
    private static readonly Error s_minLengthValueError = Error.Validation(description: "MinLengthValueError");
    private static readonly Error s_exactLengthValueError = Error.Validation(description: "ExactLengthValueError");
    private static readonly Error s_rangeLengthValueError = Error.Validation(description: "RangeLengthValueError");
    private static readonly Error s_matchValueError = Error.Validation(description: "MatchValueError");
    private static readonly Error s_selectorMatchValueError = Error.Validation(description: "SelectorMatchValueError");
    private static readonly Error s_inclusiveIntValueError = Error.Validation(description: "InclusiveIntValueError");
    private static readonly Error s_inclusiveNullableIntValueError = Error.Validation(description: "InclusiveNullableIntValueError");
    private static readonly Error s_equalValueError = Error.Validation(description: "EqualValueError");
}
