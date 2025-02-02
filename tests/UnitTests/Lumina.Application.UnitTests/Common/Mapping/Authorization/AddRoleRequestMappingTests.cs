#region ========================================================================= USING =====================================================================================
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
        Assert.NotNull(result);
        Assert.Equal(request.RoleName, result.RoleName);
        Assert.Equal(request.Permissions, result.Permissions);
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
        Assert.NotNull(result);
        Assert.Equal(request.RoleName, result.RoleName);
        Assert.Empty(result.Permissions);
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
        Assert.NotNull(result);
        Assert.Equal(roleName, result.RoleName);
        Assert.Equal(request.Permissions, result.Permissions);
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
        Assert.NotNull(result);
        Assert.Equal(request.RoleName, result.RoleName);
        Assert.Equal(permissions, result.Permissions);
        Assert.Equal(3, result.Permissions.Count);
    }
}
