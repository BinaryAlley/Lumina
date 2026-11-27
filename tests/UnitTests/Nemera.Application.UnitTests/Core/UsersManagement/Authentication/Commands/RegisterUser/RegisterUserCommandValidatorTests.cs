#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Nemera.Application.Fixtures.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();
    private readonly RegisterUserCommandFixture _registerUserCommandFixture = new();

    [Fact]
    public void Validate_WhenUsernameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        command = command with { Username = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        command = command with { PasswordConfirm = "$321Bcda" };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordsNotMatch);
    }

    [Theory]
    [InlineData("password123")] // Missing uppercase and special character.
    [InlineData("PASSWORD123")] // Missing special character.
    [InlineData("Password123")] // Missing special character.
    [InlineData("Password!@#")] // Missing number.
    [InlineData("Pass1!")] // Too short.
    [InlineData("password")] // Missing uppercase, number, and special character.
    public void Validate_WhenPasswordDoesNotMatchPattern_ShouldHaveValidationError(string password)
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        command = command with { Password = password, PasswordConfirm = password };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.InvalidPassword);
    }

    [Theory]
    [InlineData("Password1!")] // Valid password.
    [InlineData("Complex1@Password")] // Valid password.
    [InlineData("MyP@ssw0rd")] // Valid password.
    [InlineData("Abcd123!@#")] // Valid password.
    public void Validate_WhenPasswordMatchesPattern_ShouldNotHaveValidationError(string password)
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
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
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        command = command with
        {
            Username = "valid.user123",
            Password = "$321Bcda",
            PasswordConfirm = "$321Bcda"
        };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidPassword);
        result.ShouldNotHaveValidationError(Errors.Authentication.PasswordConfirmCannotBeEmpty);
    }

    [Theory]
    [InlineData("a")] // Too short.
    [InlineData("ab")] // Too short.
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // Too long (256 chars).
    public void Validate_WhenUsernameHasInvalidLength_ShouldHaveValidationError(string username)
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        command = command with { Username = username };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameMustBeBetween3And255CharactersLong);
    }

    [Theory]
    [InlineData(".username")] // Starts with special character.
    [InlineData("username.")] // Ends with special character.
    [InlineData("_username")] // Starts with special character.
    [InlineData("username_")] // Ends with special character.
    [InlineData("-username")] // Starts with special character.
    [InlineData("username-")] // Ends with special character.
    [InlineData("user name")] // Contains space.
    [InlineData("user@name")] // Contains invalid special char.
    [InlineData("user#name")] // Contains invalid special char.
    [InlineData("user$name")] // Contains invalid special char.
    public void Validate_WhenUsernameHasInvalidFormat_ShouldHaveValidationError(string username)
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        command = command with { Username = username };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.InvalidUsername);
    }

    [Theory]
    [InlineData("user123")] // Basic alphanumeric.
    [InlineData("user.name")] // With dot.
    [InlineData("user_name")] // With underscore.
    [InlineData("user-name")] // With hyphen.
    [InlineData("User.Name123")] // Mixed case with special chars and numbers.
    [InlineData("a1b")] // Minimum length.
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // Maximum length (255 chars).
    public void Validate_WhenUsernameHasValidFormat_ShouldNotHaveValidationError(string username)
    {
        // Arrange
        RegisterUserCommand command = _registerUserCommandFixture.Create();
        command = command with { Username = username };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidUsername);
    }
}
