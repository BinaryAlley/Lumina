#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Application.UnitTests.Core.Maintenance.ApplicationSetup.Commands.SetupApplication.Fixtures;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;

/// <summary>
/// Contains unit tests for the <see cref="SetupApplicationCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupApplicationCommandValidatorTests
{
    private readonly SetupApplicationCommandValidator _validator;
    private readonly SetupApplicationCommandFixture _commandFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationCommandValidatorTests"/> class.
    /// </summary>
    public SetupApplicationCommandValidatorTests()
    {
        _validator = new SetupApplicationCommandValidator();
        _commandFixture = new SetupApplicationCommandFixture();
    }

    [Fact]
    public void Validate_WhenUsernameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Username = null };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Username = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUsernameIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Username = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordIsNull_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Password = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Password = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Password = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordConfirmIsNull_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { PasswordConfirm = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordConfirmCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordConfirmIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { PasswordConfirm = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordConfirmCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordConfirmIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { PasswordConfirm = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordConfirmCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordAndPasswordConfirmDontMatch_ShouldHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { PasswordConfirm = "$321Bcda" };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordsNotMatch);
    }

    [Theory]
    [InlineData("password123")] // Missing uppercase and special character
    [InlineData("PASSWORD123")] // Missing special character
    [InlineData("Password123")] // Missing special character
    [InlineData("Password!@#")] // Missing number
    [InlineData("Pass1!")] // Too short
    [InlineData("password")] // Missing uppercase, number, and special character
    public void Validate_WhenPasswordDoesNotMatchPattern_ShouldHaveValidationError(string password)
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Password = password, PasswordConfirm = password };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.InvalidPassword);
    }

    [Theory]
    [InlineData("Password1!")] // Valid password
    [InlineData("Complex1@Password")] // Valid password
    [InlineData("MyP@ssw0rd")] // Valid password
    [InlineData("Abcd123!@#")] // Valid password
    public void Validate_WhenPasswordMatchesPattern_ShouldNotHaveValidationError(string password)
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Password = password, PasswordConfirm = password };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidPassword);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        SetupApplicationCommand command = _commandFixture.CreateSetupApplicationCommand();
        command = command with { Password = "$321Bcda", PasswordConfirm = "$321Bcda" };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidPassword);
        result.ShouldNotHaveValidationError(Errors.Authentication.PasswordConfirmCannotBeEmpty);
    }
}
