#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AuthorizationResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Serialize_WhenSerializingAuthorizationResponse_ShouldPreserveValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HashSet<AuthorizationPermission> permissions = [AuthorizationPermission.CanViewUsers, AuthorizationPermission.CanDeleteUsers];
        AuthorizationResponse expected = new(userId, "Admin", permissions);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);

        // Assert
        Assert.Contains($"\"UserId\":\"{userId}\"", json, StringComparison.Ordinal);
        Assert.Contains("\"Role\":\"Admin\"", json, StringComparison.Ordinal);
        Assert.Contains("canViewUsers", json, StringComparison.Ordinal);
        Assert.Contains("canDeleteUsers", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_WhenSerializingAuthorizationResponse_ShouldSerializePermissionsAsCamelCaseStrings()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HashSet<AuthorizationPermission> permissions = [AuthorizationPermission.CanRegisterUsers];
        AuthorizationResponse sut = new(userId, "Admin", permissions);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("canRegisterUsers", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HashSet<AuthorizationPermission> permissions = [AuthorizationPermission.None];
        AuthorizationResponse sut = new(userId, null, permissions);

        // Act
        (Guid sutUserId, string? role, IReadOnlySet<AuthorizationPermission> sutPermissions) = sut;

        // Assert
        Assert.Equal(sut.UserId, sutUserId);
        Assert.Equal(sut.Role, role);
        Assert.Equal(sut.Permissions, sutPermissions);
    }
}
