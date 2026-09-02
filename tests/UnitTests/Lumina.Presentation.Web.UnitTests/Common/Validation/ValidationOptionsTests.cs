#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Validation;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Validation;

/// <summary>
/// Contains unit tests for the <see cref="ValidationOptions{TOptions}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidationOptionsTests
{
    [Fact]
    public void Validate_WhenMatchingName_ShouldValidateOptions()
    {
        // Arrange
        string optionsName = "OptionsName";
        TestOptions options = new();
        IValidator<TestOptions> validator = Substitute.For<IValidator<TestOptions>>();
        validator.Validate(options).Returns([]);
        ValidationOptions<TestOptions> sut = new(optionsName, validator);

        // Act
        ValidateOptionsResult result = sut.Validate(optionsName, options);

        // Assert
        Assert.IsType<ValidateOptionsResult>(result);
        Assert.False(result.Failed);
        validator.Received(1).Validate(options);
    }

    [Fact]
    public void Validate_WhenNonMatchingName_ShouldSkipValidation()
    {
        // Arrange
        string optionsName = "OptionsName";
        string differentOptionsName = "DifferentOptionsName";
        TestOptions options = new();
        IValidator<TestOptions> validator = Substitute.For<IValidator<TestOptions>>();
        ValidationOptions<TestOptions> sut = new(optionsName, validator);

        // Act
        ValidateOptionsResult result = sut.Validate(differentOptionsName, options);

        // Assert
        Assert.Equal(ValidateOptionsResult.Skip, result);
        validator.DidNotReceive().Validate(options);
    }

    [Fact]
    public void Validate_WhenNullName_ShouldValidateOptions()
    {
        // Arrange
        TestOptions options = new();
        IValidator<TestOptions> validator = Substitute.For<IValidator<TestOptions>>();
        validator.Validate(options).Returns([]);
        ValidationOptions<TestOptions> sut = new(null, validator);

        // Act
        ValidateOptionsResult result = sut.Validate("AnyOptionsName", options);

        // Assert
        Assert.IsType<ValidateOptionsResult>(result);
        Assert.False(result.Failed);
        validator.Received(1).Validate(options);
    }

    [Fact]
    public void Validate_WhenNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        string optionsName = "OptionsName";
        IValidator<TestOptions> validator = Substitute.For<IValidator<TestOptions>>();
        ValidationOptions<TestOptions> sut = new(optionsName, validator);

        // Act
        Action act = () => sut.Validate(optionsName, null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
        validator.DidNotReceive().Validate(Arg.Any<TestOptions>());
    }

    [Fact]
    public void Validate_WhenInvalidOptions_ShouldReturnFailureResult()
    {
        // Arrange
        string optionsName = "OptionsName";
        TestOptions options = new();
        IValidator<TestOptions> validator = Substitute.For<IValidator<TestOptions>>();
        List<Error> validationFailures =
        [
            Error.Validation("PropertyName", "Error Message")
        ];
        validator.Validate(options).Returns(validationFailures);
        ValidationOptions<TestOptions> sut = new(optionsName, validator);

        // Act
        ValidateOptionsResult result = sut.Validate(optionsName, options);

        // Assert
        Assert.IsType<ValidateOptionsResult>(result);
        Assert.True(result.Failed);
        Assert.Single(result.Failures);
        Assert.Contains("Options validation failed for 'PropertyName' with error: 'Error Message'", result.Failures);
    }

    [Fact]
    public void Validate_WhenMultipleValidationErrors_ShouldReturnOneFailureMessagePerError()
    {
        // Arrange
        string optionsName = "OptionsName";
        TestOptions options = new();
        IValidator<TestOptions> validator = Substitute.For<IValidator<TestOptions>>();
        List<Error> validationFailures =
        [
            Error.Validation("PropertyOne", "First Error Message"),
            Error.Validation("PropertyTwo", "Second Error Message")
        ];
        validator.Validate(options).Returns(validationFailures);
        ValidationOptions<TestOptions> sut = new(optionsName, validator);

        // Act
        ValidateOptionsResult result = sut.Validate(optionsName, options);

        // Assert
        Assert.IsType<ValidateOptionsResult>(result);
        Assert.True(result.Failed);
        Assert.Equal(2, result.Failures.Count());
        Assert.Contains("Options validation failed for 'PropertyOne' with error: 'First Error Message'", result.Failures);
        Assert.Contains("Options validation failed for 'PropertyTwo' with error: 'Second Error Message'", result.Failures);
    }

    /// <summary>
    /// Placeholder options type used to exercise the generic <see cref="ValidationOptions{TOptions}"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TestOptions
    {
    }
}
