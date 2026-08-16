#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Specifications;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
#endregion

namespace Lumina.Application.Fixtures.Common.Specifications;

/// <summary>
/// Fixture class for generating a <see cref="FilterSpecification{T}"/> instance that is satisfied when an integer value exceeds a configured threshold.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class GreaterThanFilterSpecificationFixture : FilterSpecification<int>
{
    private readonly int _threshold;

    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanFilterSpecificationFixture"/> class.
    /// </summary>
    /// <param name="threshold">The threshold value that an entity must exceed to satisfy the specification.</param>
    public GreaterThanFilterSpecificationFixture(int threshold)
    {
        _threshold = threshold;
    }

    /// <summary>
    /// Creates a LINQ expression that evaluates whether a value is greater than the configured threshold.
    /// </summary>
    /// <returns>An expression tree that evaluates to <see langword="true"/> when the value exceeds the threshold.</returns>
    public override Expression<Func<int, bool>> ToExpression()
    {
        int threshold = _threshold;
        return value => value > threshold;
    }
}
