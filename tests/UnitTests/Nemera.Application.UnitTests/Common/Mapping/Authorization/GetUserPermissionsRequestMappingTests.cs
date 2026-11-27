#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using Nemera.Contracts.Fixtures.Core.Requests.Authorization;
using Nemera.Contracts.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetUserPermissionsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsRequestMappingTests
{
    private readonly GetUserPermissionsRequestFixture _getUserPermissionsRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetUserPermissionsRequest request = _getUserPermissionsRequestFixture.Create(userId);

        // Act
        GetUserPermissionsQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.UserId, result.UserId);
    }

    [Fact]
    public void ToQuery_WhenUserIdIsNull_ShouldMapCorrectly()
    {
        // Arrange
        GetUserPermissionsRequest request = _getUserPermissionsRequestFixture.Create(includeUserId: false);

        // Act
        GetUserPermissionsQuery result = request.ToQuery();

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
        GetUserPermissionsRequest request = _getUserPermissionsRequestFixture.Create(userId);

        // Act
        GetUserPermissionsQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.UserId, result.UserId);
    }

    [Fact]
    public void ToQuery_WhenMappingEmptyGuid_ShouldMapCorrectly()
    {
        // Arrange
        GetUserPermissionsRequest request = _getUserPermissionsRequestFixture.Create(Guid.Empty);

        // Act
        GetUserPermissionsQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.UserId);
    }
}
