#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Nemera.Application.Fixtures.Core.Admin.Authorization.Roles.Commands.AddRole;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.AddRole;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleCommandValidatorTests
{
    private readonly AddRoleCommandValidator _validator = new();
    private readonly AddRoleCommandFixture _addRoleCommandFixture = new();

    [Fact]
    public void Validate_WhenRoleNameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        command = command with { RoleName = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.RoleNameCannotBeNull);
    }

    [Fact]
    public void Validate_WhenRoleNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        command = command with { RoleName = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.RoleNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenRoleNameIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        command = command with { RoleName = " " };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.RoleNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPermissionsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        command = command with { Permissions = null! };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.PermissionsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenPermissionsIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        command = command with { Permissions = [] };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.PermissionsListCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPermissionContainsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        command = command with { Permissions = [Guid.Empty, Guid.NewGuid()] };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.PermissionIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
