#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Specifications;
using Lumina.Application.Fixtures.Common.Specifications;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
#endregion

namespace Lumina.Application.UnitTests.Common.Specifications;

/// <summary>
/// Contains unit tests for the <see cref="AndFilterSpecification{T}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AndFilterSpecificationTests
{
    [Fact]
    public void ToExpression_WhenBothSpecificationsSatisfyCriteria_ShouldReturnExpressionThatEvaluatesToTrue()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture left = new(5);
        GreaterThanFilterSpecificationFixture right = new(10);
        AndFilterSpecification<int> specification = new(left, right);

        // Act
        Expression<Func<int, bool>> expression = specification.ToExpression();
        Func<int, bool> predicate = expression.Compile();

        // Assert
        Assert.True(predicate(20));
    }

    [Fact]
    public void ToExpression_WhenOnlyLeftSpecificationSatisfiesCriteria_ShouldReturnExpressionThatEvaluatesToFalse()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture left = new(5);
        GreaterThanFilterSpecificationFixture right = new(10);
        AndFilterSpecification<int> specification = new(left, right);

        // Act
        Expression<Func<int, bool>> expression = specification.ToExpression();
        Func<int, bool> predicate = expression.Compile();

        // Assert
        Assert.False(predicate(8));
    }

    [Fact]
    public void ToExpression_WhenNeitherSpecificationSatisfiesCriteria_ShouldReturnExpressionThatEvaluatesToFalse()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture left = new(5);
        GreaterThanFilterSpecificationFixture right = new(10);
        AndFilterSpecification<int> specification = new(left, right);

        // Act
        Expression<Func<int, bool>> expression = specification.ToExpression();
        Func<int, bool> predicate = expression.Compile();

        // Assert
        Assert.False(predicate(2));
    }

    [Fact]
    public void IsSatisfiedBy_WhenBothSpecificationsAreSatisfied_ShouldReturnTrue()
    {
        // Arrange
        AlwaysTrueFilterSpecificationFixture<int> left = new();
        AlwaysTrueFilterSpecificationFixture<int> right = new();
        AndFilterSpecification<int> specification = new(left, right);

        // Act
        bool result = specification.IsSatisfiedBy(0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WhenEitherSpecificationIsNotSatisfied_ShouldReturnFalse()
    {
        // Arrange
        AlwaysTrueFilterSpecificationFixture<int> left = new();
        AlwaysFalseFilterSpecificationFixture<int> right = new();
        AndFilterSpecification<int> specification = new(left, right);

        // Act
        bool result = specification.IsSatisfiedBy(0);

        // Assert
        Assert.False(result);
    }
}
