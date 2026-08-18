#region ========================================================================= USING =====================================================================================
using Bogus;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.Utilities;

/// <summary>
/// Fixture class for generating <see cref="ValidatorUtilitiesTestRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatorUtilitiesTestRequestFixture
{
    private const string ValidPattern = "^[a-z]{3}-\\d{3}$";

    /// <summary>
    /// Creates a new <see cref="ValidatorUtilitiesTestRequest"/> instance with randomized test data that satisfies all validation rules.
    /// </summary>
    /// <param name="intValue">Optional. The integer value that must be greater than 3.</param>
    /// <param name="nullableIntValue">Optional. The nullable integer value that must be greater than 3.</param>
    /// <param name="decimalValue">Optional. The decimal value that must be greater than 3.</param>
    /// <param name="nullableDecimalValue">Optional. The nullable decimal value that must be greater than 3.</param>
    /// <param name="timeSpanValue">Optional. The time span value that must be greater than 3 hours.</param>
    /// <param name="intValue2">Optional. The integer value that must be greater than or equal to 5.</param>
    /// <param name="nullableIntValue2">Optional. The nullable integer value that must be greater than or equal to 5.</param>
    /// <param name="nullableDecimalValue2">Optional. The nullable decimal value that must be greater than or equal to 5.</param>
    /// <param name="notNullValue">Optional. The value that must not be null.</param>
    /// <param name="notEmptyValue">Optional. The value that must not be empty.</param>
    /// <param name="notEmptyGuidValue">Optional. The Guid that must not be empty.</param>
    /// <param name="notEmptyCollectionValue">Optional. The collection that must not be empty.</param>
    /// <param name="enumValue">Optional. The enum value that must be defined.</param>
    /// <param name="nullableEnumValue">Optional. The nullable enum value that must be defined.</param>
    /// <param name="maxLengthValue">Optional. The value that must not exceed 5 characters.</param>
    /// <param name="minLengthValue">Optional. The value that must be at least 3 characters.</param>
    /// <param name="exactLengthValue">Optional. The value that must be exactly 3 characters.</param>
    /// <param name="rangeLengthValue">Optional. The value that must be between 3 and 5 characters.</param>
    /// <param name="matchValue">Optional. The value that must match the valid pattern.</param>
    /// <param name="selectorMatchValue">Optional. The value that must match the pattern selected from the request.</param>
    /// <param name="patternValue">Optional. The pattern the selector match value must match.</param>
    /// <param name="inclusiveIntValue">Optional. The integer value that must be between 1 and 10.</param>
    /// <param name="inclusiveNullableIntValue">Optional. The nullable integer value that must be between 1 and 10.</param>
    /// <param name="equalValue">Optional. The value that must equal the comparison value.</param>
    /// <param name="equalComparisonValue">Optional. The comparison value.</param>
    /// <returns>A configured <see cref="ValidatorUtilitiesTestRequest"/> instance.</returns>
    public ValidatorUtilitiesTestRequest Create(
        int? intValue = null,
        int? nullableIntValue = null,
        decimal? decimalValue = null,
        decimal? nullableDecimalValue = null,
        TimeSpan? timeSpanValue = null,
        int? intValue2 = null,
        int? nullableIntValue2 = null,
        decimal? nullableDecimalValue2 = null,
        string? notNullValue = null,
        string? notEmptyValue = null,
        Guid? notEmptyGuidValue = null,
        List<string>? notEmptyCollectionValue = null,
        ValidatorUtilitiesTestEnum? enumValue = null,
        ValidatorUtilitiesTestEnum? nullableEnumValue = null,
        string? maxLengthValue = null,
        string? minLengthValue = null,
        string? exactLengthValue = null,
        string? rangeLengthValue = null,
        string? matchValue = null,
        string? selectorMatchValue = null,
        string? patternValue = null,
        int? inclusiveIntValue = null,
        int? inclusiveNullableIntValue = null,
        string? equalValue = null,
        string? equalComparisonValue = null)
    {
        string resolvedPattern = patternValue ?? ValidPattern;
        return new Faker<ValidatorUtilitiesTestRequest>()
            .CustomInstantiator(f => new ValidatorUtilitiesTestRequest(
                IntValue: intValue ?? f.Random.Int(4, 100),
                NullableIntValue: nullableIntValue ?? f.Random.Int(4, 100),
                DecimalValue: decimalValue ?? f.Random.Decimal(4m, 100m),
                NullableDecimalValue: nullableDecimalValue ?? f.Random.Decimal(4m, 100m),
                TimeSpanValue: timeSpanValue ?? f.Date.BetweenTimeOnly(new TimeOnly(4, 0), new TimeOnly(23, 0)).ToTimeSpan(),
                IntValue2: intValue2 ?? f.Random.Int(5, 100),
                NullableIntValue2: nullableIntValue2 ?? f.Random.Int(5, 100),
                NullableDecimalValue2: nullableDecimalValue2 ?? f.Random.Decimal(5m, 100m),
                NotNullValue: notNullValue ?? f.Lorem.Word(),
                NotEmptyValue: notEmptyValue ?? f.Lorem.Word(),
                NotEmptyGuidValue: notEmptyGuidValue ?? f.Random.Guid(),
                NotEmptyCollectionValue: notEmptyCollectionValue ?? [f.Lorem.Word()],
                EnumValue: enumValue ?? f.PickRandom<ValidatorUtilitiesTestEnum>(),
                NullableEnumValue: nullableEnumValue ?? f.PickRandom<ValidatorUtilitiesTestEnum>(),
                MaxLengthValue: maxLengthValue ?? f.Random.String2(1, 5),
                MinLengthValue: minLengthValue ?? f.Random.String2(3, 5),
                ExactLengthValue: exactLengthValue ?? f.Random.String2(3, 3),
                RangeLengthValue: rangeLengthValue ?? f.Random.String2(3, 5),
                MatchValue: matchValue ?? GenerateMatchingValue(f),
                SelectorMatchValue: selectorMatchValue ?? GenerateMatchingValue(f),
                PatternValue: resolvedPattern,
                InclusiveIntValue: inclusiveIntValue ?? f.Random.Int(1, 10),
                InclusiveNullableIntValue: inclusiveNullableIntValue ?? f.Random.Int(1, 10),
                EqualValue: equalValue ?? "same",
                EqualComparisonValue: equalComparisonValue ?? "same"))
            .Generate();
    }

    /// <summary>
    /// Creates multiple <see cref="ValidatorUtilitiesTestRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ValidatorUtilitiesTestRequest"/> instances.</returns>
    public List<ValidatorUtilitiesTestRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    private static string GenerateMatchingValue(Faker f)
    {
        string letters = new(Enumerable.Range(0, 3).Select(_ => f.Random.Char('a', 'z')).ToArray());
        return $"{letters}-{f.Random.Replace("###")}";
    }
}
