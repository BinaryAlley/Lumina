#region ========================================================================= USING =====================================================================================
using System;
using System.Linq.Expressions;
#endregion

namespace Lumina.Application.Common.Specifications;

/// <summary>
/// Represents the abstract base for a filtering specification, enabling the encapsulation of business query logic as reusable objects.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
public abstract class FilterSpecification<T>
{
    /// <summary>
    /// Creates a LINQ expression that represents the predicate defined by the current specification.
    /// </summary>
    /// <returns>An expression tree that can be used to evaluate whether an object of type <typeparamref name="T"/> satisfies the specification criteria.</returns>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Determines whether the specified entity satisfies the criteria defined by the specification.
    /// </summary>
    /// <param name="entity">The entity to evaluate against the specification criteria.</param>
    /// <returns><see langword="true"/> if the entity satisfies the specification; <see langword="false"/> otherwise.</returns>
    public bool IsSatisfiedBy(T entity)
    {
        Func<T, bool> predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// Combines the current filter specification with another specification using a logical AND operation.
    /// </summary>
    /// <param name="specification">The filter specification to combine with the current specification.</param>
    /// <returns>A new filter specification that is satisfied if both the current specification and the specified specification is satisfied.</returns>
    public FilterSpecification<T> And(FilterSpecification<T> specification)
    {
        return new AndFilterSpecification<T>(this, specification);
    }

    /// <summary>
    /// Combines the current filter specification with another specification using a logical OR operation.
    /// </summary>
    /// <param name="specification">The filter specification to combine with the current specification.</param>
    /// <returns>A new filter specification that is satisfied if either the current specification or the specified specification is satisfied.</returns>
    public FilterSpecification<T> Or(FilterSpecification<T> specification)
    {
        return new OrFilterSpecification<T>(this, specification);
    }
}
