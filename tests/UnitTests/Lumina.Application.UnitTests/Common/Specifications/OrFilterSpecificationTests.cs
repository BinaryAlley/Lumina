#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Specifications;
using Lumina.Application.Fixtures.Common.Specifications;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
#endregion

namespace Lumina.Application.UnitTests.Common.Specifications;

/// <summary>
/// Contains unit tests for the <see cref="OrFilterSpecification{T}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OrFilterSpecificationTests
{
    [Fact]
    public void ToExpression_WhenOnlyLeftSpecificationSatisfiesCriteria_ShouldReturnExpressionThatEvaluatesToTrue()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture left = new(5);
        GreaterThanFilterSpecificationFixture right = new(10);
        OrFilterSpecification<int> specification = new(left, right);

        // Act
        Expression<Func<int, bool>> expression = specification.ToExpression();
        Func<int, bool> predicate = expression.Compile();

        // Assert
        Assert.True(predicate(8));
    }

    [Fact]
    public void ToExpression_WhenOnlyRightSpecificationSatisfiesCriteria_ShouldReturnExpressionThatEvaluatesToTrue()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture left = new(10);
        GreaterThanFilterSpecificationFixture right = new(5);
        OrFilterSpecification<int> specification = new(left, right);

        // Act
        Expression<Func<int, bool>> expression = specification.ToExpression();
        Func<int, bool> predicate = expression.Compile();

        // Assert
        Assert.True(predicate(8));
    }

    [Fact]
    public void ToExpression_WhenBothSpecificationsSatisfyCriteria_ShouldReturnExpressionThatEvaluatesToTrue()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture left = new(5);
        GreaterThanFilterSpecificationFixture right = new(10);
        OrFilterSpecification<int> specification = new(left, right);

        // Act
        Expression<Func<int, bool>> expression = specification.ToExpression();
        Func<int, bool> predicate = expression.Compile();

        // Assert
        Assert.True(predicate(20));
    }

    [Fact]
    public void ToExpression_WhenNeitherSpecificationSatisfiesCriteria_ShouldReturnExpressionThatEvaluatesToFalse()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture left = new(5);
        GreaterThanFilterSpecificationFixture right = new(10);
        OrFilterSpecification<int> specification = new(left, right);

        // Act
        Expression<Func<int, bool>> expression = specification.ToExpression();
        Func<int, bool> predicate = expression.Compile();

        // Assert
        Assert.False(predicate(2));
    }

    [Fact]
    public void IsSatisfiedBy_WhenAtLeastOneSpecificationIsSatisfied_ShouldReturnTrue()
    {
        // Arrange
        AlwaysTrueFilterSpecificationFixture<int> left = new();
        AlwaysFalseFilterSpecificationFixture<int> right = new();
        OrFilterSpecification<int> specification = new(left, right);

        // Act
        bool result = specification.IsSatisfiedBy(0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WhenNeitherSpecificationIsSatisfied_ShouldReturnFalse()
    {
        // Arrange
        AlwaysFalseFilterSpecificationFixture<int> left = new();
        AlwaysFalseFilterSpecificationFixture<int> right = new();
        OrFilterSpecification<int> specification = new(left, right);

        // Act
        bool result = specification.IsSatisfiedBy(0);

        // Assert
        Assert.False(result);
    }
}
