#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetRolePermissionsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsRequestMappingTests
{
    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        GetRolePermissionsRequest request = new(roleId);

        // Act
        GetRolePermissionsQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(request.RoleId);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToQuery_WhenMappingDifferentRoleIds_ShouldMapCorrectly(string roleIdString)
    {
        // Arrange
        Guid roleId = Guid.Parse(roleIdString);
        GetRolePermissionsRequest request = new(roleId);

        // Act
        GetRolePermissionsQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void ToQuery_WhenMappingEmptyGuid_ShouldMapCorrectly()
    {
        // Arrange
        GetRolePermissionsRequest request = new(Guid.Empty);

        // Act
        GetRolePermissionsQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(Guid.Empty);
    }
}
