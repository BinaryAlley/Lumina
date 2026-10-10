#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleRequestTests
{
    private readonly AddRoleRequestFixture _addRoleRequestFixture = new();

    [Fact]
    public void Constructor_WhenPassingNullPermissions_ShouldReturnNullPermissions()
    {
        // Act
        AddRoleRequest sut = _addRoleRequestFixture.Create(roleName: "Admin", includePermissions: false);

        // Assert
        Assert.Null(sut.Permissions);
    }
}
