#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserRoleAndPermissionsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsRequestMappingTests
{
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        List<Guid> permissions = [Guid.NewGuid()];
        UpdateUserRoleAndPermissionsRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            permissions
        );

        // Act
        UpdateUserRoleAndPermissionsCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId!.Value);
        result.RoleId.Should().Be(request.RoleId);
        result.Permissions.Should().BeEquivalentTo(request.Permissions);
    }

    [Fact]
    public void ToCommand_WhenRoleIdIsNull_ShouldMapCorrectly()
    {
        // Arrange
        List<Guid> permissions = [Guid.NewGuid()];
        UpdateUserRoleAndPermissionsRequest request = new(
            Guid.NewGuid(),
            null,
            permissions
        );

        // Act
        UpdateUserRoleAndPermissionsCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId!.Value);
        result.RoleId.Should().BeNull();
        result.Permissions.Should().BeEquivalentTo(request.Permissions);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToCommand_WhenMappingDifferentUserIds_ShouldMapCorrectly(string userIdString)
    {
        // Arrange
        UpdateUserRoleAndPermissionsRequest request = new(
            Guid.Parse(userIdString),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );

        // Act
        UpdateUserRoleAndPermissionsCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId!.Value);
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
        UpdateUserRoleAndPermissionsRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            permissions
        );

        // Act
        UpdateUserRoleAndPermissionsCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Permissions.Should().BeEquivalentTo(permissions);
        result.Permissions.Should().HaveCount(3);
    }
}
