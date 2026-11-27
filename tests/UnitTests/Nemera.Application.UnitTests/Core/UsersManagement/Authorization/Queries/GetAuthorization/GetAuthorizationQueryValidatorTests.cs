#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Nemera.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Nemera.Application.Common.Errors;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetAuthorization;

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
