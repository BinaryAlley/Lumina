#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.LoginUser.Fixtures;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
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
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Username = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUsernameIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Username = "   " };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordIsNull_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = null! };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPasswordIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = "   " };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Authentication.PasswordCannotBeEmpty);
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
        List<Error> result = _validator.TestValidate(query);

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
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { Password = password };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidPassword);
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
        List<Error> result = _validator.TestValidate(query);

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
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { TotpCode = totpCode };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidTotpCode);
    }

    [Fact]
    public void Validate_WhenTotpCodeIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery();
        query = query with { TotpCode = null };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidTotpCode);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        LoginUserQuery query = LoginUserQueryFixture.CreateLoginQuery(true);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authentication.UsernameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidPassword);
        result.ShouldNotHaveValidationError(Errors.Authentication.InvalidTotpCode);
    }
}
