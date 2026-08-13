#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using Lumina.Application.UnitTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.SharedKernel.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserRoleAndPermissionsCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsCommandValidatorTests
{
    private readonly UpdateUserRoleAndPermissionsCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsCommandValidatorTests"/> class.
    /// </summary>
    public UpdateUserRoleAndPermissionsCommandValidatorTests()
    {
        _validator = new UpdateUserRoleAndPermissionsCommandValidator();
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = new(
            Guid.Empty,
            Guid.NewGuid(),
            [Guid.NewGuid()]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(DomainErrors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPermissionsContainEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = new(
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
        UpdateUserRoleAndPermissionsCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null!);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPermissionsIsEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = new(
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
        UpdateUserRoleAndPermissionsCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
