#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using NSubstitute;
using Lumina.Infrastructure.Common.Validation;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validation;

/// <summary>
/// Contains unit tests for the <see cref="FluentValidationOptions"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FluentValidationOptionsTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationOptionsTests"/> class.
    /// </summary>
    public FluentValidationOptionsTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }
    #endregion

    #region ================================================================= METHODS ===================================================================================
    [Fact]
    public void Validate_WhenMatchingName_ShouldValidateOptions()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var options = _fixture.Create<FluentValidationOptionsFixture>();
        var validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        validator.Validate(options).Returns(new ValidationResult());
        var sut = new FluentValidationOptions<FluentValidationOptionsFixture>(name, validator);

        // Act
        var result = sut.Validate(name, options);

        // Assert
        result.Should().BeOfType<ValidateOptionsResult>();
        result.Failed.Should().BeFalse();
        validator.Received(1).Validate(options);
    }

    [Fact]
    public void Validate_WhenNonMatchingName_ShouldSkipValidation()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var differentName = _fixture.Create<string>();
        var options = _fixture.Create<FluentValidationOptionsFixture>();
        var validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        var sut = new FluentValidationOptions<FluentValidationOptionsFixture>(name, validator);

        // Act
        var result = sut.Validate(differentName, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Skip);
        validator.DidNotReceive().Validate(options);
    }

    [Fact]
    public void Validate_WhenNullName_ShouldValidateOptions()
    {
        // Arrange
        var options = _fixture.Create<FluentValidationOptionsFixture>();
        var validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        validator.Validate(options).Returns(new ValidationResult());
        var sut = new FluentValidationOptions<FluentValidationOptionsFixture>(null, validator);

        // Act
        var result = sut.Validate(_fixture.Create<string>(), options);

        // Assert
        result.Should().BeOfType<ValidateOptionsResult>();
        result.Failed.Should().BeFalse();
        validator.Received(1).Validate(options);
    }

    [Fact]
    public void Validate_WhenNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        var sut = new FluentValidationOptions<FluentValidationOptionsFixture>(name, validator);

        // Act
        Action act = () => sut.Validate(name, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_WhenInvalidOptions_ShouldReturnFailureResult()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var options = _fixture.Create<FluentValidationOptionsFixture>();
        var validator = Substitute.For<IValidator<FluentValidationOptionsFixture>>();
        var validationFailures = new List<ValidationFailure>
        {
            new("PropertyName", "Error Message")
        };
        validator.Validate(options).Returns(new ValidationResult(validationFailures));
        var sut = new FluentValidationOptions<FluentValidationOptionsFixture>(name, validator);

        // Act
        var result = sut.Validate(name, options);

        // Assert
        result.Should().BeOfType<ValidateOptionsResult>();
        result.Failed.Should().BeTrue();
        // TODO: should be modified to account for translation of error messages
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Options validation failed for 'PropertyName' with error: 'Error Message'");

    }
    #endregion
}