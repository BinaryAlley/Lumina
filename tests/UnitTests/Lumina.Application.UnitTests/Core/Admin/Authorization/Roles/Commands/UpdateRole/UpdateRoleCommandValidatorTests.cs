#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.UpdateRole.Fixtures;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.UpdateRole;

/// <summary>
/// Contains unit tests for the <see cref="UpdateRoleCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleCommandValidatorTests
{
    private readonly UpdateRoleCommandValidator _validator;
    private readonly UpdateRoleCommandFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandValidatorTests"/> class.
    /// </summary>
    public UpdateRoleCommandValidatorTests()
    {
        _validator = new UpdateRoleCommandValidator();
        _fixture = new UpdateRoleCommandFixture();
    }

    [Fact]
    public void Validate_WhenRoleIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.CreateCommand();
        command = command with { RoleId = Guid.Empty };

        // Act
        TestValidationResult<UpdateRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId)
            .WithErrorMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenRoleNameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.CreateCommand();
        command = command with { RoleName = null! };

        // Act
        TestValidationResult<UpdateRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleName)
            .WithErrorMessage(Errors.Authorization.RoleNameCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenRoleNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.CreateCommand();
        command = command with { RoleName = string.Empty };

        // Act
        TestValidationResult<UpdateRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleName)
            .WithErrorMessage(Errors.Authorization.RoleNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenRoleNameIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.CreateCommand();
        command = command with { RoleName = " " };

        // Act
        TestValidationResult<UpdateRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleName)
            .WithErrorMessage(Errors.Authorization.RoleNameCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPermissionsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.CreateCommand();
        command = command with { Permissions = null! };

        // Act
        TestValidationResult<UpdateRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Permissions)
            .WithErrorMessage(Errors.Authorization.PermissionsListCannotBeNull.Description);
    }

    [Fact]
    public void Validate_WhenPermissionsIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.CreateCommand();
        command = command with { Permissions = [] };

        // Act
        TestValidationResult<UpdateRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Permissions)
            .WithErrorMessage(Errors.Authorization.PermissionsListCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPermissionContainsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.CreateCommand();
        command = command with { Permissions = [Guid.Empty, Guid.NewGuid()] };

        // Act
        TestValidationResult<UpdateRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Permissions[0]")
            .WithErrorMessage(Errors.Authorization.PermissionIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.CreateCommand();

        // Act
        TestValidationResult<UpdateRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
