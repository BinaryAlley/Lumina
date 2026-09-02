#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleRequestTests
{
    private readonly AddRoleRequestFixture _addRoleRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidAddRoleRequest()
    {
        // Act
        AddRoleRequest sut = _addRoleRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.RoleName));
    }

    [Fact]
    public void Constructor_WhenPassingNullPermissions_ShouldReturnNullPermissions()
    {
        // Act
        AddRoleRequest sut = new(RoleName: "Admin", Permissions: null);

        // Assert
        Assert.Null(sut.Permissions);
    }

    [Fact]
    public void RoundTrip_WhenSerializingAddRoleRequest_ShouldPreserveValues()
    {
        // Arrange
        AddRoleRequest expected = _addRoleRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        AddRoleRequest? actual = JsonSerializer.Deserialize<AddRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
}
