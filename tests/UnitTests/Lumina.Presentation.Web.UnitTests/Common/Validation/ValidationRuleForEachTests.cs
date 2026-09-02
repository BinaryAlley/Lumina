#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Validation;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Validation;

/// <summary>
/// Contains unit tests for the <see cref="ValidationRuleForEach{TRequest, TItem}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidationRuleForEachTests
{
    private readonly ValidationRuleForEach<TestRequest, string> _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationRuleForEachTests"/> class.
    /// </summary>
    public ValidationRuleForEachTests()
    {
        _sut = new ValidationRuleForEach<TestRequest, string>(request => request.Items);
    }

    [Fact]
    public void Validate_WhenNoPredicatesConfigured_ShouldReturnNoErrors()
    {
        // Arrange
        TestRequest request = new()
        {
            Items = ["value"]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenCollectionIsNull_ShouldReturnNoErrors()
    {
        // Arrange
        _sut.Must(item => item.Length > 0);
        TestRequest request = new()
        {
            Items = null
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenCollectionIsEmpty_ShouldReturnNoErrors()
    {
        // Arrange
        _sut.Must(item => item.Length > 0);
        TestRequest request = new()
        {
            Items = []
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenAllItemsSatisfyPredicate_ShouldReturnNoErrors()
    {
        // Arrange
        _sut.Must(item => item.Length > 0);
        TestRequest request = new()
        {
            Items = ["alpha", "beta"]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenPredicateFailsOnSomeItems_ShouldReturnErrorForEachFailingItem()
    {
        // Arrange
        _sut.Must(item => item.Length > 0);
        TestRequest request = new()
        {
            Items = ["alpha", "", "beta"]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Error error = Assert.Single(result);
        Assert.Equal(Error.Validation(), error);
    }

    [Fact]
    public void Validate_WhenPredicateFailsOnMultipleItems_ShouldReturnOneErrorPerItem()
    {
        // Arrange
        _sut.Must(item => item.Length > 0);
        TestRequest request = new()
        {
            Items = ["", ""]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, error => Assert.Equal(Error.Validation(), error));
    }

    [Fact]
    public void Validate_WhenMultiplePredicatesFailOnSameItem_ShouldReturnErrorForEachFailingPredicate()
    {
        // Arrange
        _sut.Must(item => item.Length > 3);
        _sut.Must(item => item.StartsWith('x'));
        TestRequest request = new()
        {
            Items = ["ab"]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, error => Assert.Equal(Error.Validation(), error));
    }

    [Fact]
    public void Validate_WhenInstanceAwarePredicateFails_ShouldReturnErrorForEachFailingItem()
    {
        // Arrange
        _sut.Must((request, item) => item != request.ForbiddenItem);
        TestRequest request = new()
        {
            Items = ["forbidden", "allowed"],
            ForbiddenItem = "forbidden"
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Error error = Assert.Single(result);
        Assert.Equal(Error.Validation(), error);
    }

    [Fact]
    public void Validate_WhenInstanceAwarePredicatePasses_ShouldReturnNoErrors()
    {
        // Arrange
        _sut.Must((request, item) => item != request.ForbiddenItem);
        TestRequest request = new()
        {
            Items = ["allowed", "alsoAllowed"],
            ForbiddenItem = "forbidden"
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenConditionEvaluatesToFalse_ShouldSkipValidation()
    {
        // Arrange
        _sut.When(request => request.ShouldValidate)
            .Must(item => item.Length > 0);
        TestRequest request = new()
        {
            Items = [""],
            ShouldValidate = false
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenConditionEvaluatesToTrue_ShouldRunPredicates()
    {
        // Arrange
        _sut.When(request => request.ShouldValidate)
            .Must(item => item.Length > 0);
        TestRequest request = new()
        {
            Items = [""],
            ShouldValidate = true
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Error error = Assert.Single(result);
        Assert.Equal(Error.Validation(), error);
    }

    [Fact]
    public void Validate_WhenWithErrorConfiguredForPlainPredicate_ShouldReturnConfiguredError()
    {
        // Arrange
        _sut.Must(item => item.Length > 0)
            .WithError(Error.Validation(description: "Item must not be empty."));
        TestRequest request = new()
        {
            Items = [""]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Error error = Assert.Single(result);
        Assert.Equal(Error.Validation(description: "Item must not be empty."), error);
    }

    [Fact]
    public void Validate_WhenWithErrorConfiguredForInstanceAwarePredicate_ShouldReturnConfiguredError()
    {
        // Arrange
        _sut.Must((request, item) => item != request.ForbiddenItem)
            .WithError(Error.Validation(description: "Item is forbidden."));
        TestRequest request = new()
        {
            Items = ["forbidden"],
            ForbiddenItem = "forbidden"
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Error error = Assert.Single(result);
        Assert.Equal(Error.Validation(description: "Item is forbidden."), error);
    }

    [Fact]
    public void Validate_WhenWithErrorConfiguredBeforeAnyPredicate_ShouldApplyToNextAddedPredicate()
    {
        // Arrange
        _sut.WithError(Error.Validation(description: "Configured before predicate."))
            .Must(item => item.Length > 0);
        TestRequest request = new()
        {
            Items = [""]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Error error = Assert.Single(result);
        Assert.Equal(Error.Validation(description: "Configured before predicate."), error);
    }

    [Fact]
    public void Validate_WhenWithErrorReceivesNonValidationError_ShouldReturnValidationErrorWithSameDescription()
    {
        // Arrange
        _sut.Must(item => item.Length > 0)
            .WithError(Error.NotFound(description: "Item is missing."));
        TestRequest request = new()
        {
            Items = [""]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Error error = Assert.Single(result);
        Assert.Equal(Error.Validation(description: "Item is missing."), error);
    }

    [Fact]
    public void Validate_WhenChildRuleFailsOnItem_ShouldReturnChildValidationError()
    {
        // Arrange
        _sut.ChildRules(childValidator => childValidator.RuleFor(item => item.Length).Must(length => length >= 3));
        TestRequest request = new()
        {
            Items = ["ab"]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Error error = Assert.Single(result);
        Assert.Equal(Error.Validation(), error);
    }

    [Fact]
    public void Validate_WhenChildRuleFailsOnMultipleItems_ShouldReturnOneErrorPerItem()
    {
        // Arrange
        _sut.ChildRules(childValidator => childValidator.RuleFor(item => item.Length).Must(length => length >= 3));
        TestRequest request = new()
        {
            Items = ["a", "bb"]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, error => Assert.Equal(Error.Validation(), error));
    }

    [Fact]
    public void Validate_WhenAllItemsPassChildRule_ShouldReturnNoErrors()
    {
        // Arrange
        _sut.ChildRules(childValidator => childValidator.RuleFor(item => item.Length).Must(length => length >= 3));
        TestRequest request = new()
        {
            Items = ["abc", "defg"]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenChildRuleConfiguredAndCollectionContainsNullItem_ShouldSkipNullItem()
    {
        // Arrange
        _sut.ChildRules(childValidator => childValidator.RuleFor(item => item.Length).Must(length => length >= 1));
        TestRequest request = new()
        {
            Items = ["a", null!]
        };

        // Act
        List<Error> result = [.. _sut.Validate(request)];

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Test request type used to exercise the <see cref="ValidationRuleForEach{TRequest, TItem}"/> rule.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class TestRequest
    {
        /// <summary>
        /// Gets the collection of string items validated by the rule under test.
        /// </summary>
        public List<string>? Items { get; init; }

        /// <summary>
        /// Gets a value indicating whether the rule condition should allow validation to run.
        /// </summary>
        public bool ShouldValidate { get; init; } = true;

        /// <summary>
        /// Gets the value used by the instance-aware predicate to identify forbidden items.
        /// </summary>
        public string ForbiddenItem { get; init; } = "forbidden";
    }
}
