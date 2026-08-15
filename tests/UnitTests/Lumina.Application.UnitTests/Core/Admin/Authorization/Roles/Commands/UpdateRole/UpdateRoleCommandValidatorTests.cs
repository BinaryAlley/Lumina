#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Lumina.Application.Fixtures.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
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
        UpdateRoleCommand command = _fixture.Create();
        command = command with { RoleId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Authorization.RoleIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenRoleNameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateRoleCommand command = _fixture.Create();
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
        UpdateRoleCommand command = _fixture.Create();
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
        UpdateRoleCommand command = _fixture.Create();
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
        UpdateRoleCommand command = _fixture.Create();
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
        UpdateRoleCommand command = _fixture.Create();
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
        UpdateRoleCommand command = _fixture.Create();
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
        UpdateRoleCommand command = _fixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
