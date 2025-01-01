#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleRequestMappingTests
{
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        DeleteRoleRequest request = new(roleId);

        // Act
        DeleteRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(request.RoleId!.Value);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToCommand_WhenMappingDifferentRoleIds_ShouldMapCorrectly(string roleIdString)
    {
        // Arrange
        Guid roleId = Guid.Parse(roleIdString);
        DeleteRoleRequest request = new(roleId);

        // Act
        DeleteRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void ToCommand_WhenRoleIdIsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DeleteRoleRequest request = new(null);

        // Act
        Action act = () => request.ToCommand();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToCommand_WhenMappingEmptyGuid_ShouldMapCorrectly()
    {
        // Arrange
        DeleteRoleRequest request = new(Guid.Empty);

        // Act
        DeleteRoleCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(Guid.Empty);
    }
}
