#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
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
    [Fact]
    public void ToResponse_WhenMappingValidEntity_ShouldMapCorrectly()
    {
        // Arrange
        UserAuthorizationEntity entity = new()
        {
            UserId = Guid.NewGuid(),
            Roles = new HashSet<AuthorizationRole> { AuthorizationRole.Admin },
            Permissions = new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers }
        };

        // Act
        GetAuthorizationResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(entity.UserId);
        result.Roles.Should().BeEquivalentTo(entity.Roles);
        result.Permissions.Should().BeEquivalentTo(entity.Permissions);
    }

    [Fact]
    public void ToResponse_WhenMappingEntityWithEmptyCollections_ShouldMapCorrectly()
    {
        // Arrange
        UserAuthorizationEntity entity = new()
        {
            UserId = Guid.NewGuid(),
            Roles = new HashSet<AuthorizationRole>(),
            Permissions = new HashSet<AuthorizationPermission>()
        };

        // Act
        GetAuthorizationResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(entity.UserId);
        result.Roles.Should().BeEmpty();
        result.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void ToResponse_WhenMappingEntityWithMultipleRolesAndPermissions_ShouldMapCorrectly()
    {
        // Arrange
        UserAuthorizationEntity entity = new()
        {
            UserId = Guid.NewGuid(),
            Roles = new HashSet<AuthorizationRole>
            {
                AuthorizationRole.Admin,
                AuthorizationRole.None
            },
            Permissions = new HashSet<AuthorizationPermission>
            {
                AuthorizationPermission.CanViewUsers,
                AuthorizationPermission.CanDeleteUsers,
                AuthorizationPermission.CanRegisterUsers
            }
        };

        // Act
        GetAuthorizationResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(entity.UserId);
        result.Roles.Should().BeEquivalentTo(entity.Roles);
        result.Permissions.Should().BeEquivalentTo(entity.Permissions);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToResponse_WhenMappingDifferentUserIds_ShouldMapCorrectly(string userIdString)
    {
        // Arrange
        UserAuthorizationEntity entity = new()
        {
            UserId = Guid.Parse(userIdString),
            Roles = new HashSet<AuthorizationRole> { AuthorizationRole.Admin },
            Permissions = new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers }
        };

        // Act
        GetAuthorizationResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(entity.UserId);
        result.Roles.Should().BeEquivalentTo(entity.Roles);
        result.Permissions.Should().BeEquivalentTo(entity.Permissions);
    }
}
