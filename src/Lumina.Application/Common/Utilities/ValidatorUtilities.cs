#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using System;
using System.Collections;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Application.Common.Utilities;

/// <summary>
/// Validator extension methods for the <see cref="IRuleBuilder{TRequest, TProperty}"/> to define common validation rules.
/// </summary>
public static class ValidatorUtilities
{
    /// <summary>
    /// Defines a validation rule that ensures the integer property is greater than the specified value.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="valueToCompare">The value that the property must be greater than.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, int> GreaterThan<TRequest>(this IRuleBuilder<TRequest, int> ruleBuilder, int valueToCompare)
    {
        return ruleBuilder.Must(value => value > valueToCompare);
    }

    /// <summary>
    /// Defines a validation rule that ensures the nullable integer property is greater than the specified value.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/>, this rule passes (use <see cref="NotNull{TRequest, TProperty}(IRuleBuilder{TRequest, TProperty})"/> to check for missing values).
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="valueToCompare">The value that the property must be greater than.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, int?> GreaterThan<TRequest>(this IRuleBuilder<TRequest, int?> ruleBuilder, int valueToCompare)
    {
        return ruleBuilder.Must(value => !value.HasValue || value.Value > valueToCompare);
    }

    /// <summary>
    /// Defines a validation rule that ensures the decimal property is greater than the specified value.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="valueToCompare">The value that the property must be greater than.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, decimal> GreaterThan<TRequest>(this IRuleBuilder<TRequest, decimal> ruleBuilder, decimal valueToCompare)
    {
        return ruleBuilder.Must(value => value > valueToCompare);
    }

    /// <summary>
    /// Defines a validation rule that ensures the nullable decimal property is greater than the specified value.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/>, this rule passes (use <see cref="NotNull{TRequest, TProperty}(IRuleBuilder{TRequest, TProperty})"/> to check for missing values).
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="valueToCompare">The value that the property must be greater than.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, decimal?> GreaterThan<TRequest>(this IRuleBuilder<TRequest, decimal?> ruleBuilder, decimal valueToCompare)
    {
        return ruleBuilder.Must(value => !value.HasValue || value.Value > valueToCompare);
    }

    /// <summary>
    /// Defines a validation rule that ensures the time span property is greater than the specified value.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="valueToCompare">The value that the property must be greater than.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TimeSpan> GreaterThan<TRequest>(this IRuleBuilder<TRequest, TimeSpan> ruleBuilder, TimeSpan valueToCompare)
    {
        return ruleBuilder.Must(value => value > valueToCompare);
    }

    /// <summary>
    /// Defines a validation rule that ensures the integer property is greater than or equal to the specified value.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="valueToCompare">The value that the property must be greater than or equal to.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, int> GreaterThanOrEqualTo<TRequest>(this IRuleBuilder<TRequest, int> ruleBuilder, int valueToCompare)
    {
        return ruleBuilder.Must(value => value >= valueToCompare);
    }

    /// <summary>
    /// Defines a validation rule that ensures the nullable integer property is greater than or equal to the specified value.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/>, this rule passes.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="valueToCompare">The value that the property must be greater than or equal to.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, int?> GreaterThanOrEqualTo<TRequest>(this IRuleBuilder<TRequest, int?> ruleBuilder, int valueToCompare)
    {
        return ruleBuilder.Must(value => !value.HasValue || value.Value >= valueToCompare);
    }

    /// <summary>
    /// Defines a validation rule that ensures the nullable decimal property is greater than or equal to the specified value.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/>, this rule passes.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="valueToCompare">The value that the property must be greater than or equal to.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, decimal?> GreaterThanOrEqualTo<TRequest>(this IRuleBuilder<TRequest, decimal?> ruleBuilder, decimal valueToCompare)
    {
        return ruleBuilder.Must(value => !value.HasValue || value.Value >= valueToCompare);
    }

    /// <summary>
    /// Defines a validation rule that ensures the property is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// This rule checks reference/nullable-value nullness only. Combine with
    /// <see cref="NotEmpty{TRequest, TProperty}(IRuleBuilder{TRequest, TProperty})"/> to also reject empty values.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> NotNull<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder)
    {
        return ruleBuilder.Must(value => value is not null);
    }

    /// <summary>
    /// Defines a validation rule that ensures the property is not empty.
    /// </summary>
    /// <remarks>
    /// This rule is type-aware: a <see cref="string"/> is checked for white-space only content, a <see cref="Guid"/>
    /// is checked against <see cref="Guid.Empty"/>, and any <see cref="IEnumerable"/> is checked for having at least
    /// one item. If the property is <see langword="null"/>, this rule fails.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> NotEmpty<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder)
    {
        return ruleBuilder.Must(value => value switch
        {
            null => false,
            string stringValue => stringValue.Trim().Length > 0,
            Guid guidValue => guidValue != Guid.Empty,
            IEnumerable enumerable => enumerable.GetEnumerator().MoveNext(),
            _ => true
        });
    }

    /// <summary>
    /// Defines a validation rule that ensures the value is a defined member of the specified enumeration type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TEnum> IsInEnum<TRequest, TEnum>(this IRuleBuilder<TRequest, TEnum> ruleBuilder) where TEnum : struct, Enum
    {
        return ruleBuilder.Must(value => Enum.IsDefined(value));
    }

    /// <summary>
    /// Defines a validation rule that ensures a nullable enum value is either <see langword="null"/> or a defined member of the specified enumeration type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TEnum?> IsInEnum<TRequest, TEnum>(this IRuleBuilder<TRequest, TEnum?> ruleBuilder) where TEnum : struct, Enum
    {
        return ruleBuilder.Must(value => value.HasValue && Enum.IsDefined(value.Value));
    }

    /// <summary>
    /// Defines a validation rule that ensures the string property does not exceed the specified maximum length.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/> or not a string, this rule passes.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="maximumLength">The maximum allowed length of the property.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> MaximumLength<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder, int maximumLength)
    {
        return ruleBuilder.Must(value => value is not string stringValue || stringValue.Length <= maximumLength);
    }

    /// <summary>
    /// Defines a validation rule that ensures the string property is at least the specified minimum length.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/> or not a string, this rule fails.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="minimumLength">The minimum allowed length of the property.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> MinimumLength<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder, int minimumLength)
    {
        return ruleBuilder.Must(value => value is string stringValue && stringValue.Length >= minimumLength);
    }

    /// <summary>
    /// Defines a validation rule that ensures the string property has exactly the specified length.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/> or not a string, this rule fails.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="length">The exact allowed length of the property.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> Length<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder, int length)
    {
        return ruleBuilder.Must(value => value is string stringValue && stringValue.Length == length);
    }

    /// <summary>
    /// Defines a validation rule that ensures the string property length is within the specified inclusive range.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/> or not a string, this rule fails.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="minimumLength">The minimum allowed length of the property.</param>
    /// <param name="maximumLength">The maximum allowed length of the property.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> Length<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder, int minimumLength, int maximumLength)
    {
        return ruleBuilder.Must(value => value is string stringValue && stringValue.Length >= minimumLength && stringValue.Length <= maximumLength);
    }

    /// <summary>
    /// Defines a validation rule that ensures the string property matches the specified regular expression pattern.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/> or not a string, this rule fails (use
    /// <see cref="When{TRequest, TProperty}(IRuleBuilder{TRequest, TProperty}, Func{TRequest, bool})"/> to skip optional properties).
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="regexPattern">The regular expression pattern the property must match.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> Matches<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder, string regexPattern)
    {
        return ruleBuilder.Must(value => value is string stringValue && Regex.IsMatch(stringValue, regexPattern));
    }

    /// <summary>
    /// Defines a validation rule that ensures the string property matches the regular expression pattern selected from the request instance.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/> or not a string, this rule fails.
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="regexPatternSelector">A function that selects the regular expression pattern to match against from the request instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> Matches<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder, Func<TRequest, string> regexPatternSelector)
    {
        return ruleBuilder.Must((instance, value) => value is string stringValue && Regex.IsMatch(stringValue, regexPatternSelector(instance)));
    }

    /// <summary>
    /// Defines a validation rule that ensures the integer property is within the specified inclusive range.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="minimumValue">The minimum allowed value of the property.</param>
    /// <param name="maximumValue">The maximum allowed value of the property.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, int> InclusiveBetween<TRequest>(this IRuleBuilder<TRequest, int> ruleBuilder, int minimumValue, int maximumValue)
    {
        return ruleBuilder.Must(value => value >= minimumValue && value <= maximumValue);
    }

    /// <summary>
    /// Defines a validation rule that ensures the nullable integer property is within the specified inclusive range.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/>, this rule passes (use <see cref="NotNull{TRequest, TProperty}(IRuleBuilder{TRequest, TProperty})"/> to check for missing values).
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="minimumValue">The minimum allowed value of the property.</param>
    /// <param name="maximumValue">The maximum allowed value of the property.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, int?> InclusiveBetween<TRequest>(this IRuleBuilder<TRequest, int?> ruleBuilder, int minimumValue, int maximumValue)
    {
        return ruleBuilder.Must(value => !value.HasValue || (value.Value >= minimumValue && value.Value <= maximumValue));
    }

    /// <summary>
    /// Defines a validation rule that ensures the property is equal to another property of the request.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <param name="comparisonSelector">A function that selects the property the validated property must equal.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, TProperty> Equal<TRequest, TProperty>(this IRuleBuilder<TRequest, TProperty> ruleBuilder, Func<TRequest, TProperty> comparisonSelector)
    {
        return ruleBuilder.Must((instance, value) => Equals(value, comparisonSelector(instance)));
    }
}
