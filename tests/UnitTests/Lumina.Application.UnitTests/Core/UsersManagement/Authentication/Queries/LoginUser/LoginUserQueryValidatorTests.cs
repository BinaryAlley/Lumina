#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.LoginUser.Fixtures;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.LoginUser;

/// <summary>
/// Contains unit tests for the <see cref="LoginUserQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginUserQueryValidatorTests
{
    private readonly LoginUserQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserQueryValidatorTests"/> class.
    /// </summary>
    public LoginUserQueryValidatorTests()
    {
        _validator = new LoginUserQueryValidator();
    }

    [Fact]
    public void Validate_WhenUsernameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Username = null! };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Username = string.Empty };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUsernameIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Username = "   " };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordIsNull_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = null! };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.PasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = string.Empty };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.PasswordCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPasswordIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = "   " };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(Errors.Authentication.PasswordCannotBeEmpty.Description);
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
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = password };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

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
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = password };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
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
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { TotpCode = totpCode };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

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
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { TotpCode = totpCode };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }

    [Fact]
    public void Validate_WhenTotpCodeIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { TotpCode = null };

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery(true);

        // Act
        TestValidationResult<LoginUserQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }
}
