#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationQueryValidatorTests
{
    private readonly GetAuthorizationQueryValidator _validator = new();
    private readonly GetAuthorizationQueryFixture _getAuthorizationQueryFixture = new();

    [Fact]
    public void Validate_WhenUserIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetAuthorizationQuery query = _getAuthorizationQueryFixture.Create();
        query = query with { UserId = null };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetAuthorizationQuery query = _getAuthorizationQueryFixture.Create();
        query = query with { UserId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetAuthorizationQuery query = _getAuthorizationQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Users.UserIdCannotBeEmpty);
    }
}
