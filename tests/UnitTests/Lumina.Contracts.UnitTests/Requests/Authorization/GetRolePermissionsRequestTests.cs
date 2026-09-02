#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetRolePermissionsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsRequestTests
{
    private readonly GetRolePermissionsRequestFixture _getRolePermissionsRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetRolePermissionsRequest()
    {
        // Act
        GetRolePermissionsRequest sut = _getRolePermissionsRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.RoleId);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        GetRolePermissionsRequest first = _getRolePermissionsRequestFixture.Create();
        GetRolePermissionsRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetRolePermissionsRequest_ShouldPreserveValues()
    {
        // Arrange
        GetRolePermissionsRequest expected = _getRolePermissionsRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetRolePermissionsRequest? actual = JsonSerializer.Deserialize<GetRolePermissionsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
