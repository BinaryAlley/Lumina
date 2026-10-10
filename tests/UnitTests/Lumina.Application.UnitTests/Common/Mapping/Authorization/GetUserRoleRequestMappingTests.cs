#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleRequestMappingTests
{
    private readonly GetUserRoleRequestFixture _getUserRoleRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetUserRoleRequest request = _getUserRoleRequestFixture.Create(userId);

        // Act
        GetUserRoleQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.UserId, result.UserId);
    }

    [Fact]
    public void ToQuery_WhenUserIdIsNull_ShouldMapCorrectly()
    {
        // Arrange
        GetUserRoleRequest request = _getUserRoleRequestFixture.Create(includeUserId: false);

        // Act
        GetUserRoleQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.UserId);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToQuery_WhenMappingDifferentUserIds_ShouldMapCorrectly(string userIdString)
    {
        // Arrange
        Guid userId = Guid.Parse(userIdString);
        GetUserRoleRequest request = _getUserRoleRequestFixture.Create(userId);

        // Act
        GetUserRoleQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.UserId, result.UserId);
    }

    [Fact]
    public void ToQuery_WhenMappingEmptyGuid_ShouldMapCorrectly()
    {
        // Arrange
        GetUserRoleRequest request = _getUserRoleRequestFixture.Create(Guid.Empty);

        // Act
        GetUserRoleQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.UserId);
    }
}
