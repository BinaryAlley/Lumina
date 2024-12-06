#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetAuthorization.Fixtures;
using Lumina.Domain.Common.Errors;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationQueryValidatorTests
{
    private readonly GetAuthorizationQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationQueryValidatorTests"/> class.
    /// </summary>
    public GetAuthorizationQueryValidatorTests()
    {
        _validator = new GetAuthorizationQueryValidator();
    }

    [Fact]
    public void Validate_WhenUserIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetAuthorizationQuery query = GetAuthorizationQueryFixture.CreateGetAuthorizationQuery();
        query = query with { UserId = null };

        // Act
        TestValidationResult<GetAuthorizationQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(Errors.Users.UserIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetAuthorizationQuery query = GetAuthorizationQueryFixture.CreateGetAuthorizationQuery();
        query = query with { UserId = Guid.Empty };

        // Act
        TestValidationResult<GetAuthorizationQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(Errors.Users.UserIdCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetAuthorizationQuery query = GetAuthorizationQueryFixture.CreateGetAuthorizationQuery();

        // Act
        TestValidationResult<GetAuthorizationQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}
