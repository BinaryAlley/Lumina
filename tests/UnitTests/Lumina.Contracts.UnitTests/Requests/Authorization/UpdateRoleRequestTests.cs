#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="UpdateRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleRequestTests
{
    private readonly UpdateRoleRequestFixture _updateRoleRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUpdateRoleRequest_ShouldPreserveValues()
    {
        // Arrange
        UpdateRoleRequest expected = _updateRoleRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdateRoleRequest? actual = JsonSerializer.Deserialize<UpdateRoleRequest>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingUpdateRoleRequestWithNullPermissions_ShouldPreserveNull()
    {
        // Arrange
        UpdateRoleRequest expected = _updateRoleRequestFixture.Create() with { Permissions = null };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdateRoleRequest? actual = JsonSerializer.Deserialize<UpdateRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Permissions);
    }
}
