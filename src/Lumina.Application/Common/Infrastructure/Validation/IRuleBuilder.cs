#region ========================================================================= USING =====================================================================================
using ErrorOr;
using System;
#endregion

namespace Lumina.Application.Common.Infrastructure.Validation;

/// <summary>
/// Provides a fluent API for building validation rules for commands and queries.
/// </summary>
/// <typeparam name="TRequest">The type of the request (command or query) being validated.</typeparam>
/// <typeparam name="TProperty">The type of the specific property on the request that the rule applies to.</typeparam>
public interface IRuleBuilder<TRequest, TProperty>
{
    /// <summary>
    /// Specifies a condition that the property must satisfy to be considered valid.
    /// </summary>
    /// <param name="predicate">A function to test the property value.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    IRuleBuilder<TRequest, TProperty> Must(Func<TProperty, bool> predicate);

    /// <summary>
    /// Specifies a condition that must be met for the validation rule to be executed.
    /// </summary>
    /// <param name="condition">A function evaluating the request instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    IRuleBuilder<TRequest, TProperty> When(Func<TRequest, bool> condition);

    /// <summary>
    /// Specifies a custom <see cref="Error"/> object to be returned when the validation fails.
    /// </summary>
    /// <param name="error">The specific error to return upon failure.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    IRuleBuilder<TRequest, TProperty> WithError(Error error);
}
