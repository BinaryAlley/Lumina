#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.AddRole.Fixtures;
using System;
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
        TestValidationResult<AddRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleName)
            .WithErrorMessage(Errors.Authorization.RoleNameCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenRoleNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _fixture.CreateCommand();
        command = command with { RoleName = string.Empty };

        // Act
        TestValidationResult<AddRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleName)
            .WithErrorMessage(Errors.Authorization.RoleNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenRoleNameIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _fixture.CreateCommand();
        command = command with { RoleName = " " };

        // Act
        TestValidationResult<AddRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleName)
            .WithErrorMessage(Errors.Authorization.RoleNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPermissionsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _fixture.CreateCommand();
        command = command with { Permissions = null! };

        // Act
        TestValidationResult<AddRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Permissions)
            .WithErrorMessage(Errors.Authorization.PermissionsListCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenPermissionsIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _fixture.CreateCommand();
        command = command with { Permissions = [] };

        // Act
        TestValidationResult<AddRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Permissions)
            .WithErrorMessage(Errors.Authorization.PermissionsListCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPermissionContainsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _fixture.CreateCommand();
        command = command with { Permissions = [Guid.Empty, Guid.NewGuid()] };

        // Act
        TestValidationResult<AddRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Permissions[0]")
            .WithErrorMessage(Errors.Authorization.PermissionIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddRoleCommand command = _fixture.CreateCommand();

        // Act
        TestValidationResult<AddRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
