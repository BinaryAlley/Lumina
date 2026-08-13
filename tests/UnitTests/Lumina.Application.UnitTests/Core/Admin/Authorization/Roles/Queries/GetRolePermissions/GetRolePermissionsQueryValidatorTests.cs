#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Application.UnitTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetRolePermissionsQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsQueryValidatorTests
{
    private readonly GetRolePermissionsQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsQueryValidatorTests"/> class.
    /// </summary>
    public GetRolePermissionsQueryValidatorTests()
    {
        _validator = new GetRolePermissionsQueryValidator();
    }

    [Fact]
    public void Validate_WhenRoleIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetRolePermissionsQuery query = new(Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.RoleIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenRoleIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRolePermissionsQuery query = new(Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authorization.RoleIdCannotBeEmpty);
    }
}
