# Architecture Decision Record (ADR): 0012 - Replace FluentValidation with Custom Validation Abstractions

## Status

**Accepted** (2026-08-13)

## Context

The solution uses FluentValidation to validate commands and queries at the application boundary. A review of its usage across the codebase reveals a deliberately constrained subset of its feature set:

1. **Narrow API Surface**: Only `RuleFor`, `Must`, `When`, and `WithMessage`-equivalent patterns are used. Advanced FluentValidation features - cascading rules, `RuleSets`, async validators, `SetValidator`, `Transform`, and the `AbstractValidator` inheritance tree beyond basic use - are absent throughout.

2. **`Error`-typed Results**: The solution uses `ErrorOr` as its result type. FluentValidation returns `ValidationResult` / `ValidationFailure`, requiring a translation layer on every usage site to produce `Error` values, with some information loss in the process. A first-party validator can return `ErrorOr.Error` directly, with full metadata, eliminating that friction.

3. **External Dependency Surface**: FluentValidation is a well-maintained library, but the danger of it going commercial, like other open source popular libraries in recent times, and the lack of usage of its advanced features mandates an in-house replacement.

4. **Placement in SharedKernel**: Validation logic is cross-cutting and lives in `Lumina.Application.Common.Infrastructure.Validation`. Owning the abstraction entirely means the validation model stays aligned with the domain's `Error` type without any adapter.

## Decision

Replace FluentValidation with a minimal, first-party validation framework placed in `Lumina.Application.Common.Infrastructure.Validation`. The API mirrors FluentValidation's familiar fluent surface (`RuleFor`, `Must`, `When`, `WithError`) to minimize migration friction.

### Core Abstractions

```csharp
// IValidator.cs - public contract consumed by application handlers
public interface IValidator<in TRequest>
{
    List<Error> Validate(TRequest instance);
}

// IValidationRule.cs - internal rule evaluation contract
internal interface IValidationRule<in TRequest>
{
    Error? Validate(TRequest instance);
}

// IRuleBuilder.cs - fluent configuration surface
public interface IRuleBuilder<TRequest, TProperty>
{
    IRuleBuilder<TRequest, TProperty> Must(Func<TProperty, bool> predicate);
    IRuleBuilder<TRequest, TProperty> When(Func<TRequest, bool> condition);
    IRuleBuilder<TRequest, TProperty> WithError(Error error);
}
```

### Base Validator

```csharp
// AbstractValidator.cs
public abstract class AbstractValidator<TRequest> : IValidator<TRequest>
{
    private readonly List<IValidationRule<TRequest>> _rules = [];

    protected IRuleBuilder<TRequest, TProperty> RuleFor<TProperty>(
        Func<TRequest, TProperty> propertySelector)
    {
        ValidationRule<TRequest, TProperty> rule = new(propertySelector);
        _rules.Add(rule);
        return rule;
    }

    public List<Error> Validate(TRequest instance)
    {
        List<Error> errors = [];
        foreach (IValidationRule<TRequest> rule in _rules)
        {
            Error? error = rule.Validate(instance);
            if (error.HasValue)
                errors.Add(error.Value);
        }
        return errors;
    }
}
```

### Concrete Rule Implementation

```csharp
// ValidationRule.cs - internal sealed, not exposed to consumers
internal sealed class ValidationRule<TRequest, TProperty>
    : IValidationRule<TRequest>, IRuleBuilder<TRequest, TProperty>
{
    private readonly Func<TRequest, TProperty> _propertySelector;
    private readonly List<Func<TProperty, bool>> _predicates = [];
    private Error _error = Error.Validation();
    private Func<TRequest, bool>? _condition;

    public ValidationRule(Func<TRequest, TProperty> propertySelector)
        => _propertySelector = propertySelector;

    public IRuleBuilder<TRequest, TProperty> Must(Func<TProperty, bool> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }

    public IRuleBuilder<TRequest, TProperty> When(Func<TRequest, bool> condition)
    {
        _condition = condition;
        return this;
    }

    public IRuleBuilder<TRequest, TProperty> WithError(Error error)
    {
        _error = error;
        return this;
    }

    public Error? Validate(TRequest instance)
    {
        if (_condition is not null && !_condition(instance))
            return null;

        TProperty value = _propertySelector(instance);
        foreach (var predicate in _predicates)
            if (!predicate(value))
                return _error;

        return null;
    }
}
```

### Built-in Extension Methods

Common predicates are provided as extension methods on `IRuleBuilder<TRequest, TProperty>` via `ValidatorUtilities`, placed in `Lumina.Application.Common.Utilities`:

| Extension | Overloads | Behaviour |
|---|---|---|
| `NotEmpty()` | `string`, `List<TItem>` | Fails if `null`, empty, or whitespace-only |
| `GreaterThan(value)` | `int`, `int?`, `TimeSpan` | Fails if property ≤ value; nullable variant passes on `null` |
| `IsInEnum()` | `TEnum`, `TEnum?` | Fails if value is not a defined enum member; nullable variant passes on `null` |

### Usage Example

```csharp
internal sealed class CreateSensorCommandValidator : AbstractValidator<CreateSensorCommand>
{
    public CreateSensorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithError(Errors.Sensor.NameRequired);

        RuleFor(x => x.IntervalSeconds)
            .GreaterThan(0)
            .WithError(Errors.Sensor.InvalidInterval);

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithError(Errors.Sensor.InvalidType);

        RuleFor(x => x.ThresholdOverride)
            .GreaterThan(0)
            .When(x => x.ThresholdOverride.HasValue)
            .WithError(Errors.Sensor.InvalidThreshold);
    }
}
```

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| `Error` Alignment | Validators return `List<Error>` directly - no `ValidationResult` → `Error` translation layer at call sites |
| Dependency Reduction | Removes FluentValidation NuGet package and all transitive dependencies |
| API Ownership | Full control over rule semantics, error shape, and future extension |
| Familiar Surface | `RuleFor` / `Must` / `When` / `WithError` pattern is intentionally close to FluentValidation, minimising cognitive load during migration |
| Encapsulation | `IValidationRule` and `ValidationRule` are `internal`; consumers only interact with `IValidator` and `IRuleBuilder` |

### Risks and Mitigations

| Risk | Mitigation Strategy |
|---|---|
| Missing built-in rules surfacing post-migration | Audit all existing FluentValidation rule usages before removal; add corresponding `ValidatorUtilities` extensions as needed |
| Async validation not supported | No async validators exist, because this pertains to application use cases validation concerns (user input, etc), not business rules validations or invariants, which might require asynchronous or IO operations |
| Loss of FluentValidation test helpers (`TestValidate`) | Replace with direct `ValidatorTestExtensions.TestValidate` calls in unit tests |

## Alternatives Considered

No alternative third-party validation libraries were evaluated, as this decision was not about replacing the FluentValidation libary with another library. The solution's validation usage is sufficiently narrow (property-level predicates, conditional execution, typed error results) that the cost of adopting and adapting another external library exceeds the cost of ownership of a purpose-built implementation aligned with the `ErrorOr` result model already in use.
