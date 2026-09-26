#region ========================================================================= USING =====================================================================================
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
    [Fact]
    public void Constructor_WhenPassingNullPermissions_ShouldReturnNullPermissions()
    {
        // Act
        AddRoleRequest sut = new(RoleName: "Admin", Permissions: null);

        // Assert
        Assert.Null(sut.Permissions);
    }
}
