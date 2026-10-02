#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationRequestMappingTests
{
    private readonly GetAuthorizationRequestFixture _getAuthorizationRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetAuthorizationRequest request = _getAuthorizationRequestFixture.Create(userId);

        // Act
        GetAuthorizationQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.UserId, result.UserId);
    }

    [Fact]
    public void ToQuery_WhenUserIdIsNull_ShouldMapCorrectly()
    {
        // Arrange
        GetAuthorizationRequest request = _getAuthorizationRequestFixture.Create(includeUserId: false);

        // Act
        GetAuthorizationQuery result = request.ToQuery();

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
        GetAuthorizationRequest request = _getAuthorizationRequestFixture.Create(userId);

        // Act
        GetAuthorizationQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.UserId, result.UserId);
    }
}
