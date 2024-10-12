#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Lumina.Infrastructure.Common.Validation;
using Lumina.Infrastructure.UnitTests.Common.Validation.Fixtures;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validation;

/// <summary>
/// Contains unit tests for the <see cref="FluentValidationOptions"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FluentValidationOptionsTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationOptionsTests"/> class.
    /// </summary>
    public FluentValidationOptionsTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void Validate_WhenMatchingName_ShouldValidateOptions()
    {
        // Arrange
        string name = _fixture.Create<string>();
        FluentValidationOptionsFixture options = _fixture.Create<FluentValidationOptionsFixture>();
        IValidator<FluentValidationOptionsFixture> validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        validator.Validate(options).Returns(new ValidationResult());
        FluentValidationOptions<FluentValidationOptionsFixture> sut = new(name, validator);

        // Act
        ValidateOptionsResult result = sut.Validate(name, options);

        // Assert
        result.Should().BeOfType<ValidateOptionsResult>();
        result.Failed.Should().BeFalse();
        validator.Received(1).Validate(options);
    }

    [Fact]
    public void Validate_WhenNonMatchingName_ShouldSkipValidation()
    {
        // Arrange
        string name = _fixture.Create<string>();
        string differentName = _fixture.Create<string>();
        FluentValidationOptionsFixture options = _fixture.Create<FluentValidationOptionsFixture>();
        IValidator<FluentValidationOptionsFixture> validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        FluentValidationOptions<FluentValidationOptionsFixture> sut = new(name, validator);

        // Act
        ValidateOptionsResult result = sut.Validate(differentName, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Skip);
        validator.DidNotReceive().Validate(options);
    }

    [Fact]
    public void Validate_WhenNullName_ShouldValidateOptions()
    {
        // Arrange
        FluentValidationOptionsFixture options = _fixture.Create<FluentValidationOptionsFixture>();
        IValidator<FluentValidationOptionsFixture> validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        validator.Validate(options).Returns(new ValidationResult());
        FluentValidationOptions<FluentValidationOptionsFixture> sut = new(null, validator);

        // Act
        ValidateOptionsResult result = sut.Validate(_fixture.Create<string>(), options);

        // Assert
        result.Should().BeOfType<ValidateOptionsResult>();
        result.Failed.Should().BeFalse();
        validator.Received(1).Validate(options);
    }

    [Fact]
    public void Validate_WhenNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        string name = _fixture.Create<string>();
        IValidator<FluentValidationOptionsFixture> validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        FluentValidationOptions<FluentValidationOptionsFixture> sut = new(name, validator);

        // Act
        Action act = () => sut.Validate(name, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_WhenInvalidOptions_ShouldReturnFailureResult()
    {
        // Arrange
        string name = _fixture.Create<string>();
        FluentValidationOptionsFixture options = _fixture.Create<FluentValidationOptionsFixture>();
        IValidator<FluentValidationOptionsFixture> validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        List<ValidationFailure> validationFailures =
        [
            new("PropertyName", "Error Message")
        ];
        validator.Validate(options).Returns(new ValidationResult(validationFailures));
        FluentValidationOptions<FluentValidationOptionsFixture> sut = new(name, validator);

        // Act
        ValidateOptionsResult result = sut.Validate(name, options);

        // Assert
        result.Should().BeOfType<ValidateOptionsResult>();
        result.Failed.Should().BeTrue();
        // TODO: should be modified to account for translation of error messages
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Options validation failed for 'PropertyName' with error: 'Error Message'");

    }
}