#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="UserAuthorizationEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserAuthorizationEntityMappingTests
{
    private readonly UserAuthorizationEntityFixture _userAuthorizationEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidEntity_ShouldMapCorrectly()
    {
        // Arrange
        UserAuthorizationEntity entity = _userAuthorizationEntityFixture.Create(
            role: "Admin",
            permissions: new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers });

        // Act
        AuthorizationResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.UserId, result.UserId);
        Assert.Equal(entity.Role, result.Role);
        Assert.Equal(entity.Permissions, result.Permissions);
    }

    [Fact]
    public void ToResponse_WhenMappingEntityWithEmptyCollections_ShouldMapCorrectly()
    {
        // Arrange
        UserAuthorizationEntity entity = _userAuthorizationEntityFixture.Create(
            role: string.Empty,
            permissions: new HashSet<AuthorizationPermission>());

        // Act
        AuthorizationResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.UserId, result.UserId);
        Assert.Empty(result.Role!);
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public void ToResponse_WhenMappingEntityWithMultipleRolesAndPermissions_ShouldMapCorrectly()
    {
        // Arrange
        UserAuthorizationEntity entity = _userAuthorizationEntityFixture.Create(
            role: "Admin",
            permissions: new HashSet<AuthorizationPermission>
            {
                AuthorizationPermission.CanViewUsers,
                AuthorizationPermission.CanDeleteUsers,
                AuthorizationPermission.CanRegisterUsers
            });

        // Act
        AuthorizationResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.UserId, result.UserId);
        Assert.Equal(entity.Role, result.Role);
        Assert.Equal(entity.Permissions, result.Permissions);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToResponse_WhenMappingDifferentUserIds_ShouldMapCorrectly(string userIdString)
    {
        // Arrange
        UserAuthorizationEntity entity = _userAuthorizationEntityFixture.Create(
            userId: Guid.Parse(userIdString),
            role: "Admin",
            permissions: new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers });

        // Act
        AuthorizationResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.UserId, result.UserId);
        Assert.Equal(entity.Role, result.Role);
        Assert.Equal(entity.Permissions, result.Permissions);
    }
}
