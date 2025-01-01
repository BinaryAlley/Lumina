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
/// Contains unit tests for the <see cref="PermissionEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionEntityMappingTests
{
    [Fact]
    public void ToResponse_WhenMappingValidEntity_ShouldMapCorrectly()
    {
        // Arrange
        PermissionEntity entity = new()
        {
            Id = Guid.NewGuid(),
            PermissionName = AuthorizationPermission.CanViewUsers
        };

        // Act
        PermissionResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.PermissionName.Should().Be(entity.PermissionName);
    }

    [Theory]
    [InlineData(AuthorizationPermission.None)]
    [InlineData(AuthorizationPermission.CanViewUsers)]
    [InlineData(AuthorizationPermission.CanDeleteUsers)]
    [InlineData(AuthorizationPermission.CanRegisterUsers)]
    public void ToResponse_WhenMappingDifferentPermissions_ShouldMapCorrectly(AuthorizationPermission permission)
    {
        // Arrange
        PermissionEntity entity = new()
        {
            Id = Guid.NewGuid(),
            PermissionName = permission
        };

        // Act
        PermissionResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.PermissionName.Should().Be(permission);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleEntities_ShouldMapCorrectly()
    {
        // Arrange
        List<PermissionEntity> entities =
        [
            new() { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanViewUsers },
            new() { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanDeleteUsers },
            new() { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanRegisterUsers }
        ];

        // Act
        IEnumerable<PermissionResponse> results = entities.ToResponses();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results.Should().BeEquivalentTo(entities, options => options
            .Including(x => x.Id)
            .Including(x => x.PermissionName));
    }

    [Fact]
    public void ToResponses_WhenMappingEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        List<PermissionEntity> entities = [];

        // Act
        IEnumerable<PermissionResponse> results = entities.ToResponses();

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToResponse_WhenMappingDifferentIds_ShouldMapCorrectly(string idString)
    {
        // Arrange
        PermissionEntity entity = new()
        {
            Id = Guid.Parse(idString),
            PermissionName = AuthorizationPermission.CanViewUsers
        };

        // Act
        PermissionResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.PermissionName.Should().Be(entity.PermissionName);
    }
}
