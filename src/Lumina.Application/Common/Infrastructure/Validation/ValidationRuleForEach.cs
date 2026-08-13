#region ========================================================================= USING =====================================================================================
using ErrorOr;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Infrastructure.Validation;

/// <summary>
/// Defines a validation rule for commands and queries that evaluates each item within a collection property of a request.
/// </summary>
/// <typeparam name="TRequest">The type of the request (command or query) being validated.</typeparam>
/// <typeparam name="TItem">The type of the items in the collection that the rule applies to.</typeparam>
internal sealed class ValidationRuleForEach<TRequest, TItem> : IValidationRule<TRequest>, IRuleBuilder<TRequest, TItem>
{
    private readonly Func<TRequest, IEnumerable<TItem>?> _collectionSelector;
    private readonly List<Func<TItem, bool>> _predicates = [];
    private Error error = Error.Validation();
    private Func<TRequest, bool>? condition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationRuleForEach{TRequest, TItem}"/> class.
    /// </summary>
    /// <param name="collectionSelector">A function that extracts the collection to iterate from the request instance.</param>
    public ValidationRuleForEach(Func<TRequest, IEnumerable<TItem>?> collectionSelector)
    {
        _collectionSelector = collectionSelector;
    }

    /// <summary>
    /// Adds a condition that each item in the collection must satisfy to be considered valid.
    /// </summary>
    /// <param name="predicate">A function to test each item value.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TItem}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TItem> Must(Func<TItem, bool> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Specifies a condition that must be met for the validation rule to be executed.
    /// </summary>
    /// <param name="condition">A function evaluating the request instance.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TItem}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TItem> When(Func<TRequest, bool> condition)
    {
        this.condition = condition;
        return this;
    }

    /// <summary>
    /// Specifies a custom <see cref="Error"/> object to be returned when the validation fails.
    /// </summary>
    /// <param name="error">The specific error to return upon failure.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TItem}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TItem> WithError(Error error)
    {
        this.error = error;
        return this;
    }

    /// <summary>
    /// Evaluates all configured predicates against each item in the collection.
    /// </summary>
    /// <param name="instance">The request instance to validate.</param>
    /// <returns>One <see cref="Error"/> per item that fails a predicate; empty if every item passes.</returns>
    public IEnumerable<Error> Validate(TRequest instance)
    {
        // evaluate the condition first, if it returns false, skip validation entirely.
        if (condition is not null && !condition(instance))
            yield break;

        IEnumerable<TItem>? collection = _collectionSelector(instance);
        if (collection is null)
            yield break;

        foreach (TItem item in collection)
        {
            foreach (Func<TItem, bool> predicate in _predicates)
            {
                if (!predicate(item))
                {
                    yield return error;
                    break;
                }
            }
        }
    }
}
