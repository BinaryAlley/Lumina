#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using System;
using System.Collections.Generic;
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
    /// Defines a validation rule that ensures the property is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// This rule checks reference/nullable-value nullness only. Combine with
    /// <see cref="NotEmpty{TRequest}(IRuleBuilder{TRequest, string})"/> or
    /// <see cref="NotEmpty{TRequest, TItem}(IRuleBuilder{TRequest, List{TItem}})"/> to also reject empty values.
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
    /// Defines a validation rule that ensures the string property is not empty or consisting only of white-space characters.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/>, this rule passes (use <see cref="NotNull{TRequest, TProperty}(IRuleBuilder{TRequest, TProperty})"/> to check for missing values).
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, string> NotEmpty<TRequest>(this IRuleBuilder<TRequest, string> ruleBuilder)
    {
        return ruleBuilder.Must(value => value is null || value.Trim().Length > 0);
    }

    /// <summary>
    /// Defines a validation rule that ensures the List property is not empty.
    /// </summary>
    /// <remarks>
    /// If the property is <see langword="null"/>, this rule passes (use <see cref="NotNull{TRequest, TProperty}(IRuleBuilder{TRequest, TProperty})"/> to check for missing values).
    /// </remarks>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <typeparam name="TItem">The type of the items in the list being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, List<TItem>> NotEmpty<TRequest, TItem>(this IRuleBuilder<TRequest, List<TItem>> ruleBuilder)
    {
        return ruleBuilder.Must(value => value is null || value.Count > 0);
    }

    /// <summary>
    /// Defines a validation rule that ensures the <see cref="Guid"/> property is not <see cref="Guid.Empty"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public static IRuleBuilder<TRequest, Guid> NotEmpty<TRequest>(this IRuleBuilder<TRequest, Guid> ruleBuilder)
    {
        return ruleBuilder.Must(value => value != Guid.Empty);
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
}
