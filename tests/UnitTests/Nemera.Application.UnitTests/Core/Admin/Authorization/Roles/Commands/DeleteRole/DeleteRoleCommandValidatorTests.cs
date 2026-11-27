#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Nemera.Application.Fixtures.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleCommandValidatorTests
{
    private readonly DeleteRoleCommandValidator _validator = new();
    private readonly DeleteRoleCommandFixture _deleteRoleCommandFixture = new();

    [Fact]
    public void Validate_WhenRoleIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        DeleteRoleCommand command = _deleteRoleCommandFixture.Create(Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.RoleIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenRoleIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteRoleCommand command = _deleteRoleCommandFixture.Create(Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Authorization.RoleIdCannotBeEmpty);
    }
}
