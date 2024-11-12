#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RecoverPassword.Fixtures;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RecoverPassword;

/// <summary>
/// Contains unit tests for the <see cref="RecoverPasswordCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordCommandValidatorTests
{
    private readonly RecoverPasswordCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordCommandValidatorTests"/> class.
    /// </summary>
    public RecoverPasswordCommandValidatorTests()
    {
        _validator = new RecoverPasswordCommandValidator();
    }

    [Fact]
    public void Validate_WhenUsernameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();
        command = command with { Username = null! };

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();
        command = command with { Username = string.Empty };

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUsernameIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();
        command = command with { Username = "   " };

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenTotpCodeIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();
        command = command with { TotpCode = null! };

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotpCode)
            .WithErrorMessage(Errors.Authentication.TotpCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenTotpCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();
        command = command with { TotpCode = string.Empty };

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotpCode)
            .WithErrorMessage(Errors.Authentication.TotpCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenTotpCodeIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();
        command = command with { TotpCode = "   " };

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotpCode)
            .WithErrorMessage(Errors.Authentication.TotpCannotBeEmpty.Description);
    }

    [Theory]
    [InlineData("12345")] // Too short
    [InlineData("1234567")] // Too long
    [InlineData("abcdef")] // Contains letters
    [InlineData("12345a")] // Contains letters
    [InlineData("12.345")] // Contains special characters
    [InlineData("12 345")] // Contains whitespace
    public void Validate_WhenTotpCodeIsInvalid_ShouldHaveValidationError(string totpCode)
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();
        command = command with { TotpCode = totpCode };

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotpCode)
            .WithErrorMessage(Errors.Authentication.InvalidTotpCode.Description);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("000000")]
    [InlineData("999999")]
    public void Validate_WhenTotpCodeIsValid_ShouldNotHaveValidationError(string totpCode)
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();
        command = command with { TotpCode = totpCode };

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = RecoverPasswordCommandFixture.CreateRecoverPasswordCommand();

        // Act
        TestValidationResult<RecoverPasswordCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }
}
