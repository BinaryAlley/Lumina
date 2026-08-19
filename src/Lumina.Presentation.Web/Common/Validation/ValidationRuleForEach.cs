#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Primitives;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Validation;

/// <summary>
/// Defines a validation rule that evaluates each item within a collection property of an instance.
/// </summary>
/// <typeparam name="TRequest">The type of the instance being validated.</typeparam>
/// <typeparam name="TItem">The type of the items in the collection that the rule applies to.</typeparam>
internal sealed class ValidationRuleForEach<TRequest, TItem> : IValidationRule<TRequest>, IRuleBuilder<TRequest, TItem>
{
    private readonly Func<TRequest, IEnumerable<TItem>?> _collectionSelector;
    private readonly List<Func<TItem, bool>> _predicates = [];
    private readonly List<Error> _predicateErrors = [];
    private readonly List<Func<TRequest, TItem, bool>> _instanceAwarePredicates = [];
    private readonly List<Error> _instanceAwarePredicateErrors = [];
    private Error currentError = Error.Validation();
    private Func<TRequest, bool>? condition;
    private bool lastAddedPredicateIsInstanceAware;
    private AbstractValidator<TItem>? _childValidator;

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
        _predicateErrors.Add(currentError);
        lastAddedPredicateIsInstanceAware = false;
        return this;
    }

    /// <summary>
    /// Adds a condition that the request instance together with each item must satisfy to be considered valid.
    /// </summary>
    /// <param name="predicate">A function to test the request instance together with each item value.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TItem}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TItem> Must(Func<TRequest, TItem, bool> predicate)
    {
        _instanceAwarePredicates.Add(predicate);
        _instanceAwarePredicateErrors.Add(currentError);
        lastAddedPredicateIsInstanceAware = true;
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
        // validation rules always produce validation errors, regardless of the error type of the source error
        Error validationError = Error.Validation(description: error.Description);
        currentError = validationError;
        if (lastAddedPredicateIsInstanceAware && _instanceAwarePredicateErrors.Count > 0)
            _instanceAwarePredicateErrors[^1] = validationError;
        else if (!lastAddedPredicateIsInstanceAware && _predicateErrors.Count > 0)
            _predicateErrors[^1] = validationError;
        return this;
    }

    /// <summary>
    /// Defines nested validation rules for each item of the collection being validated.
    /// </summary>
    /// <param name="configure">An action used to configure the inline validator for each item.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TItem}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TItem> ChildRules(Action<AbstractValidator<TItem>> configure)
    {
        InlineValidator<TItem> childValidator = new();
        configure(childValidator);
        _childValidator = childValidator;
        return this;
    }

    /// <summary>
    /// Evaluates all configured predicates against each item in the collection.
    /// </summary>
    /// <param name="instance">The request instance to validate.</param>
    /// <returns>One <see cref="Error"/> per failing predicate, per item; empty if every item passes.</returns>
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
            for (int index = 0; index < _predicates.Count; index++)
                if (!_predicates[index](item))
                    yield return _predicateErrors[index];

            for (int index = 0; index < _instanceAwarePredicates.Count; index++)
                if (!_instanceAwarePredicates[index](instance, item))
                    yield return _instanceAwarePredicateErrors[index];

            if (_childValidator is not null && item is not null)
                foreach (Error childError in _childValidator.Validate(item))
                    yield return childError;
        }
    }
}
