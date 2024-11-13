#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RegisterUser.Fixture;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandValidatorTests"/> class.
    /// </summary>
    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Validate_WhenUsernameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        command = command with { PasswordConfirm = "$321Bcda" };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.PasswordsNotMatch.Description);
    }

    [Theory]
    [InlineData("password123")] // missing uppercase and special character
    [InlineData("PASSWORD123")] // missing special character
    [InlineData("Password123")] // missing special character
    [InlineData("Password!@#")] // missing number
    [InlineData("Pass1!")] // too short
    [InlineData("password")] // missing uppercase, number, and special character
    public void Validate_WhenPasswordDoesNotMatchPattern_ShouldHaveValidationError(string password)
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        command = command with { Password = password, PasswordConfirm = password };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.InvalidPassword.Description);
    }

    [Theory]
    [InlineData("Password1!")] // valid password
    [InlineData("Complex1@Password")] // valid password
    [InlineData("MyP@ssw0rd")] // valid password
    [InlineData("Abcd123!@#")] // valid password
    public void Validate_WhenPasswordMatchesPattern_ShouldNotHaveValidationError(string password)
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
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
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        command = command with
        {
            Username = "valid.user123",
            Password = "$321Bcda",
            PasswordConfirm = "$321Bcda"
        };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.PasswordConfirm);
    }

    [Theory]
    [InlineData("a")] // too short
    [InlineData("ab")] // too short
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // too long (256 chars)
    public void Validate_WhenUsernameHasInvalidLength_ShouldHaveValidationError(string username)
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        command = command with { Username = username };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameMustBeBetween3And255CharactersLong.Description);
    }

    [Theory]
    [InlineData(".username")] // starts with special character
    [InlineData("username.")] // ends with special character
    [InlineData("_username")] // starts with special character
    [InlineData("username_")] // ends with special character
    [InlineData("-username")] // starts with special character
    [InlineData("username-")] // ends with special character
    [InlineData("user name")] // contains space
    [InlineData("user@name")] // contains invalid special char
    [InlineData("user#name")] // contains invalid special char
    [InlineData("user$name")] // contains invalid special char
    public void Validate_WhenUsernameHasInvalidFormat_ShouldHaveValidationError(string username)
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        command = command with { Username = username };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("user123")] // basic alphanumeric
    [InlineData("user.name")] // with dot
    [InlineData("user_name")] // with underscore
    [InlineData("user-name")] // with hyphen
    [InlineData("User.Name123")] // mixed case with special chars and numbers
    [InlineData("a1b")] // minimum length
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // maximum length (255 chars)
    public void Validate_WhenUsernameHasValidFormat_ShouldNotHaveValidationError(string username)
    {
        // Arrange
        RegisterUserCommand command = RegisterUserCommandFixture.CreateRegisterCommand();
        command = command with { Username = username };

        // Act
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }    
}
