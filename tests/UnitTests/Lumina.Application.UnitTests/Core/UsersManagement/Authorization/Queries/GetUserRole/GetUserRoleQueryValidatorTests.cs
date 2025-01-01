#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Domain.Common.Errors;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserRole;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleQueryValidatorTests
{
    private readonly GetUserRoleQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleQueryValidatorTests"/> class.
    /// </summary>
    public GetUserRoleQueryValidatorTests()
    {
        _validator = new GetUserRoleQueryValidator();
    }

    [Fact]
    public void Validate_WhenUserIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetUserRoleQuery query = new(null);

        // Act
        TestValidationResult<GetUserRoleQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(Errors.Users.UserIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetUserRoleQuery query = new(Guid.Empty);

        // Act
        TestValidationResult<GetUserRoleQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(Errors.Users.UserIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetUserRoleQuery query = new(Guid.NewGuid());

        // Act
        TestValidationResult<GetUserRoleQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}
