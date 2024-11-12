#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.ChangePassword.Fixtures;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.ChangePassword;

/// <summary>
/// Contains unit tests for the <see cref="ChangePasswordCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCommandValidatorTests"/> class.
    /// </summary>
    public ChangePasswordCommandValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }

    [Fact]
    public void Validate_WhenUsernameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { Username = null! };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { Username = string.Empty };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUsernameIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { Username = "   " };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCurrentPasswordIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { CurrentPassword = null! };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage(Errors.Authentication.CurrentPasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCurrentPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { CurrentPassword = string.Empty };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage(Errors.Authentication.CurrentPasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCurrentPasswordIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { CurrentPassword = "   " };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage(Errors.Authentication.CurrentPasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenNewPasswordIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { NewPassword = null! };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(Errors.Authentication.NewPasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenNewPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { NewPassword = string.Empty };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(Errors.Authentication.NewPasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenNewPasswordIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { NewPassword = "   " };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(Errors.Authentication.NewPasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenNewPasswordConfirmIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { NewPasswordConfirm = null! };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPasswordConfirm)
            .WithErrorMessage(Errors.Authentication.NewPasswordConfirmCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenNewPasswordConfirmIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { NewPasswordConfirm = string.Empty };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPasswordConfirm)
            .WithErrorMessage(Errors.Authentication.NewPasswordConfirmCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenNewPasswordConfirmIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { NewPasswordConfirm = "   " };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPasswordConfirm)
            .WithErrorMessage(Errors.Authentication.NewPasswordConfirmCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenNewPasswordAndNewPasswordConfirmDontMatch_ShouldHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { NewPasswordConfirm = "$321Bcda" };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(Errors.Authentication.PasswordsNotMatch.Description);
    }

    [Theory]
    [InlineData("password123")] // Missing uppercase and special character
    [InlineData("PASSWORD123")] // Missing special character
    [InlineData("Password123")] // Missing special character
    [InlineData("Password!@#")] // Missing number
    [InlineData("Pass1!")] // Too short
    [InlineData("password")] // Missing uppercase, number, and special character
    public void Validate_WhenCurrentPasswordDoesNotMatchPattern_ShouldHaveValidationError(string password)
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { CurrentPassword = password };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage(Errors.Authentication.InvalidPassword.Description);
    }

    [Theory]
    [InlineData("password123")] // Missing uppercase and special character
    [InlineData("PASSWORD123")] // Missing special character
    [InlineData("Password123")] // Missing special character
    [InlineData("Password!@#")] // Missing number
    [InlineData("Pass1!")] // Too short
    [InlineData("password")] // Missing uppercase, number, and special character
    public void Validate_WhenNewPasswordDoesNotMatchPattern_ShouldHaveValidationError(string password)
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with { NewPassword = password, NewPasswordConfirm = password };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(Errors.Authentication.InvalidPassword.Description);
    }

    [Theory]
    [InlineData("Password1!")] // Valid password
    [InlineData("Complex1@Password")] // Valid password
    [InlineData("MyP@ssw0rd")] // Valid password
    [InlineData("Abcd123!@#")] // Valid password
    public void Validate_WhenPasswordsMatchPattern_ShouldNotHaveValidationError(string password)
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();
        command = command with
        {
            CurrentPassword = password,
            NewPassword = password,
            NewPasswordConfirm = password
        };

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        ChangePasswordCommand command = ChangePasswordCommandFixture.CreateChangePasswordCommand();

        // Act
        TestValidationResult<ChangePasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPasswordConfirm);
    }
}
