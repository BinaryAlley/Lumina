#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Specifications;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
#endregion

namespace Lumina.Application.Fixtures.Common.Specifications;

/// <summary>
/// Fixture class for generating a <see cref="FilterSpecification{T}"/> instance that is always satisfied, regardless of the entity being evaluated.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
[ExcludeFromCodeCoverage]
public sealed class AlwaysTrueFilterSpecificationFixture<T> : FilterSpecification<T>
{
    /// <summary>
    /// Creates a LINQ expression that always evaluates to <see langword="true"/>.
    /// </summary>
    /// <returns>An expression tree that always evaluates to <see langword="true"/>.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        return entity => true;
    }
}
