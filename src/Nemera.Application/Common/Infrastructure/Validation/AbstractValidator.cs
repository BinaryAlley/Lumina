#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Infrastructure.Validation;

/// <summary>
/// Base class for creating validators for commands and queries.
/// </summary>
/// <typeparam name="TRequest">The type of the request (command or query) to be validated.</typeparam>
public abstract class AbstractValidator<TRequest> : IValidator<TRequest>
{
    private readonly List<IValidationRule<TRequest>> _rules = [];

    /// <summary>
    /// Defines a validation rule for a specific property on the request being validated.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="propertySelector">A function that selects the property to validate from the request instance.</param>
    /// <returns>An <see cref="IRuleBuilder{TRequest, TProperty}"/> instance to configure the rule fluently.</returns>
    public IRuleBuilder<TRequest, TProperty> RuleFor<TProperty>(Func<TRequest, TProperty> propertySelector)
    {
        ValidationRule<TRequest, TProperty> rule = new(propertySelector);
        _rules.Add(rule);
        return rule;
    }

    /// <summary>
    /// Defines a validation rule that is evaluated against each item within a collection property on the request being validated.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection being validated.</typeparam>
    /// <param name="collectionSelector">A function that selects the collection to validate from the request instance.</param>
    /// <returns>An <see cref="IRuleBuilder{TRequest, TItem}"/> instance to configure the per-item rule fluently.</returns>
    public IRuleBuilder<TRequest, TItem> RuleForEach<TItem>(Func<TRequest, IEnumerable<TItem>?> collectionSelector)
    {
        ValidationRuleForEach<TRequest, TItem> rule = new(collectionSelector);
        _rules.Add(rule);
        return rule;
    }

    /// <summary>
    /// Validates the specified request instance against all defined rules.
    /// </summary>
    /// <param name="instance">The request instance to validate.</param>
    /// <returns>A list of validation errors. If the instance is valid, the list will be empty.</returns>
    public List<Error> Validate(TRequest instance)
    {
        List<Error> errors = [];

        foreach (IValidationRule<TRequest> rule in _rules)
            errors.AddRange(rule.Validate(instance));

        return errors;
    }
}
