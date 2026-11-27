#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using Nemera.Application.Fixtures.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserRoleAndPermissionsCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsCommandValidatorTests
{
    private readonly UpdateUserRoleAndPermissionsCommandValidator _validator = new();
    private readonly UpdateUserRoleAndPermissionsCommandFixture _updateUserRoleAndPermissionsCommandFixture = new();

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.Create(
            Guid.Empty,
            Guid.NewGuid(),
            [Guid.NewGuid()]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(ApplicationErrors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPermissionsContainEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.Empty, Guid.NewGuid()]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPermissionsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            includePermissions: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPermissionsIsEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            []);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
