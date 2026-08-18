#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserRoleAndPermissionsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsRequestTests
{
    private readonly UpdateUserRoleAndPermissionsRequestFixture _updateUserRoleAndPermissionsRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUpdateUserRoleAndPermissionsRequest_ShouldPreserveValues()
    {
        // Arrange
        UpdateUserRoleAndPermissionsRequest expected = _updateUserRoleAndPermissionsRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdateUserRoleAndPermissionsRequest? actual = JsonSerializer.Deserialize<UpdateUserRoleAndPermissionsRequest>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithOptionalValuesOmitted_ShouldPreserveNulls()
    {
        // Arrange
        UpdateUserRoleAndPermissionsRequest expected = _updateUserRoleAndPermissionsRequestFixture.Create() with { RoleId = null, Permissions = null };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        UpdateUserRoleAndPermissionsRequest? actual = JsonSerializer.Deserialize<UpdateUserRoleAndPermissionsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.RoleId);
        Assert.Null(actual.Permissions);
    }
}
