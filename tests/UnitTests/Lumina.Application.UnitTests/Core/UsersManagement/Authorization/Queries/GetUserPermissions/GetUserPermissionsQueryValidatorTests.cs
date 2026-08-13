#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.SharedKernel.Common.Errors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserPermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetUserPermissionsQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsQueryValidatorTests
{
    private readonly GetUserPermissionsQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPermissionsQueryValidatorTests"/> class.
    /// </summary>
    public GetUserPermissionsQueryValidatorTests()
    {
        _validator = new GetUserPermissionsQueryValidator();
    }

    [Fact]
    public void Validate_WhenUserIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetUserPermissionsQuery query = new(null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetUserPermissionsQuery query = new(Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetUserPermissionsQuery query = new(Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }
}
