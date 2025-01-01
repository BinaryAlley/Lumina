#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleRequestMappingTests
{
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        AddRoleRequest request = new(
            "Admin",
            [Guid.NewGuid(), Guid.NewGuid()]
        );

        // Act
        AddRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be(request.RoleName);
        result.Permissions.Should().BeEquivalentTo(request.Permissions);
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithEmptyPermissions_ShouldMapCorrectly()
    {
        // Arrange
        AddRoleRequest request = new(
            "Guest",
            []
        );

        // Act
        AddRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be(request.RoleName);
        result.Permissions.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    [InlineData("Guest")]
    public void ToCommand_WhenMappingDifferentRoleNames_ShouldMapCorrectly(string roleName)
    {
        // Arrange
        AddRoleRequest request = new(
            roleName,
            [Guid.NewGuid()]
        );

        // Act
        AddRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be(roleName);
        result.Permissions.Should().BeEquivalentTo(request.Permissions);
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithMultiplePermissions_ShouldMapCorrectly()
    {
        // Arrange
        List<Guid> permissions =
        [
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Guid.Parse("00000000-0000-0000-0000-000000000003")
        ];
        AddRoleRequest request = new("SuperAdmin", permissions);

        // Act
        AddRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be(request.RoleName);
        result.Permissions.Should().BeEquivalentTo(permissions);
        result.Permissions.Should().HaveCount(3);
    }
}
