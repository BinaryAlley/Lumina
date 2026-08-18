#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.Fixtures.Common.Utilities;

/// <summary>
/// Request used to exercise the <see cref="Lumina.Application.Common.Utilities.ValidatorUtilities"/> extension methods.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ValidatorUtilitiesTestRequest(
    int IntValue,
    int? NullableIntValue,
    decimal DecimalValue,
    decimal? NullableDecimalValue,
    TimeSpan TimeSpanValue,
    int IntValue2,
    int? NullableIntValue2,
    decimal? NullableDecimalValue2,
    string? NotNullValue,
    string? NotEmptyValue,
    Guid NotEmptyGuidValue,
    List<string>? NotEmptyCollectionValue,
    ValidatorUtilitiesTestEnum EnumValue,
    ValidatorUtilitiesTestEnum? NullableEnumValue,
    string? MaxLengthValue,
    string? MinLengthValue,
    string? ExactLengthValue,
    string? RangeLengthValue,
    string? MatchValue,
    string? SelectorMatchValue,
    string PatternValue,
    int InclusiveIntValue,
    int? InclusiveNullableIntValue,
    string? EqualValue,
    string? EqualComparisonValue);
