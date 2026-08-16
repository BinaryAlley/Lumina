#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Specifications;
using Lumina.Application.Fixtures.Common.Specifications;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
#endregion

namespace Lumina.Application.UnitTests.Common.Specifications;

/// <summary>
/// Contains unit tests for the <see cref="FilterSpecification{T}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FilterSpecificationTests
{
    [Fact]
    public void ToExpression_WhenCalledOnConcreteSpecification_ShouldReturnExpressionMatchingCriteria()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture specification = new(10);

        // Act
        Expression<Func<int, bool>> expression = specification.ToExpression();
        Func<int, bool> predicate = expression.Compile();

        // Assert
        Assert.True(predicate(15));
        Assert.False(predicate(5));
    }

    [Fact]
    public void IsSatisfiedBy_WhenEntitySatisfiesCriteria_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture specification = new(10);
        int entity = 20;

        // Act
        bool result = specification.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WhenEntityDoesNotSatisfyCriteria_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanFilterSpecificationFixture specification = new(10);
        int entity = 5;

        // Act
        bool result = specification.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void And_WhenBothSpecificationsAreSatisfied_ShouldReturnSpecificationSatisfiedByEntity()
    {
        // Arrange
        AlwaysTrueFilterSpecificationFixture<int> left = new();
        AlwaysTrueFilterSpecificationFixture<int> right = new();

        // Act
        FilterSpecification<int> combined = left.And(right);

        // Assert
        Assert.IsType<AndFilterSpecification<int>>(combined);
        Assert.True(combined.IsSatisfiedBy(0));
    }

    [Fact]
    public void And_WhenOneSpecificationIsNotSatisfied_ShouldReturnSpecificationNotSatisfiedByEntity()
    {
        // Arrange
        AlwaysTrueFilterSpecificationFixture<int> left = new();
        AlwaysFalseFilterSpecificationFixture<int> right = new();

        // Act
        FilterSpecification<int> combined = left.And(right);

        // Assert
        Assert.False(combined.IsSatisfiedBy(0));
    }

    [Fact]
    public void And_WhenBothSpecificationsAreNotSatisfied_ShouldReturnSpecificationNotSatisfiedByEntity()
    {
        // Arrange
        AlwaysFalseFilterSpecificationFixture<int> left = new();
        AlwaysFalseFilterSpecificationFixture<int> right = new();

        // Act
        FilterSpecification<int> combined = left.And(right);

        // Assert
        Assert.False(combined.IsSatisfiedBy(0));
    }

    [Fact]
    public void Or_WhenAtLeastOneSpecificationIsSatisfied_ShouldReturnSpecificationSatisfiedByEntity()
    {
        // Arrange
        AlwaysTrueFilterSpecificationFixture<int> left = new();
        AlwaysFalseFilterSpecificationFixture<int> right = new();

        // Act
        FilterSpecification<int> combined = left.Or(right);

        // Assert
        Assert.True(combined.IsSatisfiedBy(0));
    }

    [Fact]
    public void Or_WhenNeitherSpecificationIsSatisfied_ShouldReturnSpecificationNotSatisfiedByEntity()
    {
        // Arrange
        AlwaysFalseFilterSpecificationFixture<int> left = new();
        AlwaysFalseFilterSpecificationFixture<int> right = new();

        // Act
        FilterSpecification<int> combined = left.Or(right);

        // Assert
        Assert.False(combined.IsSatisfiedBy(0));
    }
}
