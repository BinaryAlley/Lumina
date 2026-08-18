#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="PermissionResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionResponseTests
{
    private readonly PermissionResponseFixture _permissionResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingPermissionResponse_ShouldPreserveValues()
    {
        // Arrange
        PermissionResponse expected = _permissionResponseFixture.Create(permissionName: AuthorizationPermission.CanCreateLibraries);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PermissionResponse? actual = JsonSerializer.Deserialize<PermissionResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingPermissionResponse_ShouldSerializePermissionNameAsCamelCaseString()
    {
        // Arrange
        PermissionResponse sut = _permissionResponseFixture.Create(permissionName: AuthorizationPermission.CanCreateLibraries);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"PermissionName\":\"canCreateLibraries\"", json, StringComparison.Ordinal);
    }
}
