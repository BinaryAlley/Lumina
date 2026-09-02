#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleRequestTests
{
    private readonly DeleteRoleRequestFixture _deleteRoleRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidDeleteRoleRequest()
    {
        // Act
        DeleteRoleRequest sut = _deleteRoleRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.True(sut.RoleId.HasValue);
    }

    [Fact]
    public void Constructor_WhenPassingNullRoleId_ShouldReturnNullRoleId()
    {
        // Act
        DeleteRoleRequest sut = new(RoleId: null);

        // Assert
        Assert.Null(sut.RoleId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingDeleteRoleRequest_ShouldPreserveValues()
    {
        // Arrange
        DeleteRoleRequest expected = _deleteRoleRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        DeleteRoleRequest? actual = JsonSerializer.Deserialize<DeleteRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
