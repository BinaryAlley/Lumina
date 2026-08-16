#region ========================================================================= USING =====================================================================================
using System;
using System.Linq.Expressions;
#endregion

namespace Lumina.Application.Common.Specifications;

/// <summary>
/// Represents a filter specification that combines two filter specifications using a logical AND operation.
/// </summary>
/// <typeparam name="T">The type of entity to which the filter specification is applied.</typeparam>
internal class AndFilterSpecification<T> : FilterSpecification<T>
{
    private readonly FilterSpecification<T> _left;
    private readonly FilterSpecification<T> _right;

    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="left">The first filter specification to combine.</param>
    /// <param name="right">The second filter specification to combine.</param>
    public AndFilterSpecification(FilterSpecification<T> left, FilterSpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    /// <summary>
    /// Creates a LINQ expression that represents the predicate defined by the current specification.
    /// </summary>
    /// <returns>An expression tree that can be used to evaluate whether an object of type <typeparamref name="T"/> satisfies the specification criteria.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpression = _left.ToExpression();
        Expression<Func<T, bool>> rightExpression = _right.ToExpression();

        ParameterExpression parameter = Expression.Parameter(typeof(T));
        BinaryExpression combined = Expression.AndAlso(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}
