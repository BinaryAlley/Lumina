#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Application.UnitTests.Core.UsersManagement.Register.Fixture;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Register;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandValidatorTests"/> class.
    /// </summary>
    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Validate_WhenUsernameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Username = null! };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Username = string.Empty };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUsernameIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Username = "   " };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Password = null! };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.PasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Password = string.Empty };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.PasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Password = "   " };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.PasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordConfirmIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { PasswordConfirm = null! };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PasswordConfirm)
            .WithErrorMessage(Errors.Authentication.PasswordConfirmCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordConfirmIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { PasswordConfirm = string.Empty };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PasswordConfirm)
            .WithErrorMessage(Errors.Authentication.PasswordConfirmCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordConfirmIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { PasswordConfirm = "   " };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PasswordConfirm)
            .WithErrorMessage(Errors.Authentication.PasswordConfirmCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordAndPasswordConfirmDontMatch_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { PasswordConfirm = "$321Bcda" };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.PasswordsNotMatch.Description);
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
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Password = password, PasswordConfirm = password };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.InvalidPassword.Description);
    }

    [Theory]
    [InlineData("Password1!")] // Valid password
    [InlineData("Complex1@Password")] // Valid password
    [InlineData("MyP@ssw0rd")] // Valid password
    [InlineData("Abcd123!@#")] // Valid password
    public void Validate_WhenPasswordMatchesPattern_ShouldNotHaveValidationError(string password)
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Password = password, PasswordConfirm = password };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterCommandFixture.CreateRegisterCommand();
        command = command with { Password = "$321Bcda", PasswordConfirm = "$321Bcda" };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.PasswordConfirm);
    }
}
