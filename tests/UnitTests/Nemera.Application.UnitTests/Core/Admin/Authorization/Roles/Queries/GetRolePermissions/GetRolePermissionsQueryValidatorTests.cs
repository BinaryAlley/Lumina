#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Nemera.Application.Fixtures.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetRolePermissionsQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsQueryValidatorTests
{
    private readonly GetRolePermissionsQueryValidator _validator = new();
    private readonly GetRolePermissionsQueryFixture _getRolePermissionsQueryFixture = new();

    [Fact]
    public void Validate_WhenRoleIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.Create(Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.RoleIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenRoleIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.Create(Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authorization.RoleIdCannotBeEmpty);
    }
}
