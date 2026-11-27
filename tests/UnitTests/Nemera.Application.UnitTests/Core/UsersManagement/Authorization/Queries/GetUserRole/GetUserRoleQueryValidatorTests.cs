#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Nemera.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Nemera.Application.Common.Errors;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserRole;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleQueryValidatorTests
{
    private readonly GetUserRoleQueryValidator _validator = new();
    private readonly GetUserRoleQueryFixture _getUserRoleQueryFixture = new();

    [Fact]
    public void Validate_WhenUserIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.Create(includeUserId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.Create(Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.Create(Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }
}
