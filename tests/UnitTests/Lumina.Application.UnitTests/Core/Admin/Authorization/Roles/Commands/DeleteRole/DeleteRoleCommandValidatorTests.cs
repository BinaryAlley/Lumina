#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Common.Errors;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleCommandValidatorTests
{
    private readonly DeleteRoleCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleCommandValidatorTests"/> class.
    /// </summary>
    public DeleteRoleCommandValidatorTests()
    {
        _validator = new DeleteRoleCommandValidator();
    }

    [Fact]
    public void Validate_WhenRoleIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        DeleteRoleCommand command = new(Guid.Empty);

        // Act
        TestValidationResult<DeleteRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId)
            .WithErrorMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenRoleIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteRoleCommand command = new(Guid.NewGuid());

        // Act
        TestValidationResult<DeleteRoleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RoleId);
    }
}
