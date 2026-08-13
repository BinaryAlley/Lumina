#region ========================================================================= USING =====================================================================================
using ErrorOr;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Infrastructure.Validation;

/// <summary>
/// Defines a validation rule for commands and queries that evaluates a specific property of a request.
/// </summary>
/// <typeparam name="TRequest">The type of the request (command or query) being validated.</typeparam>
/// <typeparam name="TProperty">The type of the specific property on the request that the rule applies to.</typeparam>
internal sealed class ValidationRule<TRequest, TProperty> : IValidationRule<TRequest>, IRuleBuilder<TRequest, TProperty>
{
    private readonly Func<TRequest, TProperty> _propertySelector;
    private readonly List<Func<TProperty, bool>> _predicates = [];
    private Error error = Error.Validation();
    private Func<TRequest, bool>? condition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationRule{TRequest, TProperty}"/> class.
    /// </summary>
    /// <param name="propertySelector">A function that extracts the property value from the request instance.</param>
    public ValidationRule(Func<TRequest, TProperty> propertySelector)
    {
        _propertySelector = propertySelector;
    }

    /// <summary>
    /// Adds a condition that the property must satisfy to be considered valid.
    /// </summary>
    /// <param name="predicate">A function to test the property value.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TProperty> Must(Func<TProperty, bool> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Specifies a condition that must be met for the validation rule to be executed.
    /// </summary>
    /// <param name="condition">A function evaluating the request instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TProperty> When(Func<TRequest, bool> condition)
    {
        this.condition = condition;
        return this;
    }

    /// <summary>
    /// Specifies a custom <see cref="Error"/> object to be returned when the validation fails.
    /// </summary>
    /// <param name="error">The specific error to return upon failure.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TProperty> WithError(Error error)
    {
        this.error = error;
        return this;
    }

    /// <summary>
    /// Evaluates all configured predicates against the specified request instance.
    /// </summary>
    /// <param name="instance">The request instance to validate.</param>
    /// <returns>The <see cref="Error"/> if any predicate fails; empty if the rule passes.</returns>
    public IEnumerable<Error> Validate(TRequest instance)
    {
        // evaluate the condition first, if it returns false, skip validation entirely.
        if (condition is not null && !condition(instance))
            yield break;

        TProperty value = _propertySelector(instance);

        foreach (Func<TProperty, bool> predicate in _predicates)
        {
            if (!predicate(value))
            {
                yield return error;
                yield break;
            }
        }
    }
}
