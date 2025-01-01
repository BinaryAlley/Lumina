#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using System;
using System.Diagnostics.CodeAnalysis;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
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
        TestValidationResult<UpdateUserRoleAndPermissionsCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Users.UserIdCannotBeEmpty.Description);
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
        TestValidationResult<UpdateUserRoleAndPermissionsCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Permissions[0]")
            .WithErrorMessage(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty.Description);
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
        TestValidationResult<UpdateUserRoleAndPermissionsCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Permissions);
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
        TestValidationResult<UpdateUserRoleAndPermissionsCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Permissions);
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
        TestValidationResult<UpdateUserRoleAndPermissionsCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
