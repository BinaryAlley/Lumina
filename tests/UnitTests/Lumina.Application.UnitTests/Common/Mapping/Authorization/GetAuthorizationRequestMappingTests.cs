#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
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
    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetAuthorizationRequest request = new(userId);

        // Act
        GetAuthorizationQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId);
    }

    [Fact]
    public void ToQuery_WhenUserIdIsNull_ShouldMapCorrectly()
    {
        // Arrange
        GetAuthorizationRequest request = new(null);

        // Act
        GetAuthorizationQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().BeNull();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToQuery_WhenMappingDifferentUserIds_ShouldMapCorrectly(string userIdString)
    {
        // Arrange
        Guid userId = Guid.Parse(userIdString);
        GetAuthorizationRequest request = new(userId);

        // Act
        GetAuthorizationQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId);
    }
}
