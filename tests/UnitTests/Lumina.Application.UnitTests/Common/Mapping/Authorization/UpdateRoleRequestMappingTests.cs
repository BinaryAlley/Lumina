#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="UpdateRoleRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleRequestMappingTests
{
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        List<Guid> permissions = [Guid.NewGuid(), Guid.NewGuid()];
        UpdateRoleRequest request = new(
            Guid.NewGuid(),
            "Admin",
            permissions
        );

        // Act
        UpdateRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(request.RoleId!.Value);
        result.RoleName.Should().Be(request.RoleName);
        result.Permissions.Should().BeEquivalentTo(request.Permissions);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    [InlineData("Guest")]
    public void ToCommand_WhenMappingDifferentRoleNames_ShouldMapCorrectly(string roleName)
    {
        // Arrange
        UpdateRoleRequest request = new(
            Guid.NewGuid(),
            roleName,
            [Guid.NewGuid()]
        );

        // Act
        UpdateRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be(roleName);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToCommand_WhenMappingDifferentRoleIds_ShouldMapCorrectly(string roleIdString)
    {
        // Arrange
        UpdateRoleRequest request = new(
            Guid.Parse(roleIdString),
            "Admin",
            [Guid.NewGuid()]
        );

        // Act
        UpdateRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(request.RoleId!.Value);
    }

    [Fact]
    public void ToCommand_WhenMappingMultiplePermissions_ShouldMapCorrectly()
    {
        // Arrange
        List<Guid> permissions =
        [
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Guid.Parse("00000000-0000-0000-0000-000000000003")
        ];
        UpdateRoleRequest request = new(Guid.NewGuid(), "Admin", permissions);

        // Act
        UpdateRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Permissions.Should().BeEquivalentTo(permissions);
        result.Permissions.Should().HaveCount(3);
    }
}
