#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.AddRole.Fixtures;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.AddRole;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleCommandValidatorTests
{
    private readonly AddRoleCommandValidator _validator;
    private readonly AddRoleCommandFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleCommandValidatorTests"/> class.
    /// </summary>
    public AddRoleCommandValidatorTests()
    {
        _validator = new AddRoleCommandValidator();
        _fixture = new AddRoleCommandFixture();
    }

    [Fact]
    public void Validate_WhenRoleNameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _fixture.CreateCommand();
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
        AddRoleCommand command = _fixture.CreateCommand();
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
        AddRoleCommand command = _fixture.CreateCommand();
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
        AddRoleCommand command = _fixture.CreateCommand();
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
        AddRoleCommand command = _fixture.CreateCommand();
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
        AddRoleCommand command = _fixture.CreateCommand();
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
        AddRoleCommand command = _fixture.CreateCommand();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
