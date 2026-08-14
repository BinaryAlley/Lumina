#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
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

    // Predicates and their errors are kept in two parallel lists (rather than a single list of tuples)
    // so that Must() can append cheaply and WithError() can patch the *last* entry by index, without
    // having to rebuild a composite struct each time.
    private readonly List<Func<TProperty, bool>> _predicates = [];
    private readonly List<Error> _predicateErrors = [];

    // instance-aware predicates get their own separate pair of lists instead of being merged into the
    // lists above, because they have a different delegate signature (TRequest, TProperty) vs (TProperty).
    // Keeping them separate avoids wrapping every plain predicate in an adapter, just to fit one shape.
    private readonly List<Func<TRequest, TProperty, bool>> _instanceAwarePredicates = [];
    private readonly List<Error> _instanceAwarePredicateErrors = [];

    private Error currentError = Error.Validation();
    private Func<TRequest, bool>? condition;

    // Tracks which list WithError() should patch, since it doesn't know on its own whether the
    // most recent Must() call went to the plain-predicate list or the instance-aware list.
    private bool lastAddedPredicateIsInstanceAware;

    private AbstractValidator<TProperty>? _childValidator;

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
        // Every predicate is paired with whatever error is "current" at the time it's added, so that a
        // subsequent WithError() call has something concrete to overwrite via index lookup.
        _predicateErrors.Add(currentError);
        lastAddedPredicateIsInstanceAware = false;
        return this;
    }

    /// <summary>
    /// Adds a condition that the request instance together with the property must satisfy to be considered valid.
    /// </summary>
    /// <param name="predicate">A function to test the request instance together with the property value.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TProperty> Must(Func<TRequest, TProperty, bool> predicate)
    {
        // This overload exists for cross-field rules (e.g. "EndDate must be after StartDate") where
        // the check can't be expressed from the property value alone and needs the whole request.
        _instanceAwarePredicates.Add(predicate);
        _instanceAwarePredicateErrors.Add(currentError);
        lastAddedPredicateIsInstanceAware = true;
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
        // Callers might pass in an error created with the wrong factory (e.g. Error.NotFound()) by mistake.
        // Rebuilding it as Error.Validation() here guarantees every error surfaced by this class is
        // consistently typed as a validation error, regardless of what the caller supplied.
        Error validationError = Error.Validation(description: error.Description);
        currentError = validationError;

        // WithError() is meant to apply to the predicate that was *just* added via Must(), not to all
        // predicates registered so far. Since Must() can target either parallel-list pair, we use the
        // flag set by the last Must() call to figure out which list's last slot to overwrite.
        if (lastAddedPredicateIsInstanceAware && _instanceAwarePredicateErrors.Count > 0)
            _instanceAwarePredicateErrors[^1] = validationError;
        else if (!lastAddedPredicateIsInstanceAware && _predicateErrors.Count > 0)
            _predicateErrors[^1] = validationError;
        return this;
    }

    /// <summary>
    /// Defines nested validation rules for the complex property being validated.
    /// </summary>
    /// <param name="configure">An action used to configure the inline validator for the property.</param>
    /// <returns>The current <see cref="IRuleBuilder{TRequest, TProperty}"/> instance for method chaining.</returns>
    public IRuleBuilder<TRequest, TProperty> ChildRules(Action<AbstractValidator<TProperty>> configure)
    {
        // Lets a rule delegate to a fully separate validator for a complex nested object (e.g. validating
        // an Address property's own fields) instead of forcing every nested check to be flattened into
        // Must() predicates on this rule.
        InlineValidator<TProperty> childValidator = new();
        configure(childValidator);
        _childValidator = childValidator;
        return this;
    }

    /// <summary>
    /// Evaluates all configured predicates against the specified request instance.
    /// </summary>
    /// <param name="instance">The request instance to validate.</param>
    /// <returns>The <see cref="Error"/> of every predicate that fails; empty if the rule passes.</returns>
    public IEnumerable<Error> Validate(TRequest instance)
    {
        // evaluate the condition first, if it returns false, skip validation entirely.
        if (condition is not null && !condition(instance))
            yield break;

        TProperty value = _propertySelector(instance);

        // Deliberately does NOT stop at the first failure: every failing predicate contributes its own error,
        // so a caller sees the full set of problems for // this property in one pass instead of having to fix
        // and resubmit one error at a time.
        for (int index = 0; index < _predicates.Count; index++)
            if (!_predicates[index](value))
                yield return _predicateErrors[index];
     
        for (int index = 0; index < _instanceAwarePredicates.Count; index++)
            if (!_instanceAwarePredicates[index](instance, value))
                yield return _instanceAwarePredicateErrors[index];
     
        if (_childValidator is not null && value is not null)
            foreach (Error childError in _childValidator.Validate(value))
                yield return childError;
    }
}
