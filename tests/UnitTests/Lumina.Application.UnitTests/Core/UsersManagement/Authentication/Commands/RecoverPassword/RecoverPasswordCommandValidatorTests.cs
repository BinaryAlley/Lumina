#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Application.Fixtures.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
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
    private readonly RecoverPasswordCommandFixture _recoverPasswordCommandFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordCommandValidatorTests"/> class.
    /// </summary>
    public RecoverPasswordCommandValidatorTests()
    {
        _validator = new RecoverPasswordCommandValidator();
        _recoverPasswordCommandFixture = new RecoverPasswordCommandFixture();
    }

    [Fact]
    public void Validate_WhenUsernameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();
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
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();
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
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();
        command = command with { Username = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTotpCodeIsNull_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();
        command = command with { TotpCode = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.TotpCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTotpCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();
        command = command with { TotpCode = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.TotpCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTotpCodeIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();
        command = command with { TotpCode = "   " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.TotpCannotBeEmpty);
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
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();
        command = command with { TotpCode = totpCode };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.InvalidTotpCode);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("000000")]
    [InlineData("999999")]
    public void Validate_WhenTotpCodeIsValid_ShouldNotHaveValidationError(string totpCode)
    {
        // Arrange
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();
        command = command with { TotpCode = totpCode };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidTotpCode);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        RecoverPasswordCommand command = _recoverPasswordCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Authentication.TotpCannotBeEmpty);
    }
}
