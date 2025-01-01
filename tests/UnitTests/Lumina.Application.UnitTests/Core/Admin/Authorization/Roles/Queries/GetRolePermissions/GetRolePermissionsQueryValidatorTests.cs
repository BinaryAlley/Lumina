#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using System;
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
        TestValidationResult<GetRolePermissionsQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId)
            .WithErrorMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenRoleIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRolePermissionsQuery query = new(Guid.NewGuid());

        // Act
        TestValidationResult<GetRolePermissionsQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RoleId);
    }
}
