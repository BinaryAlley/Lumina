#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.SharedKernel.Common.Errors;
using System;
using System.Collections.Generic;
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
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetUserRoleQuery query = new(Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetUserRoleQuery query = new(Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }
}
