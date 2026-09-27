#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleRequestTests
{
    [Fact]
    public void Constructor_WhenPassingNullRoleId_ShouldReturnNullRoleId()
    {
        // Act
        DeleteRoleRequest sut = new(RoleId: null);

        // Assert
        Assert.Null(sut.RoleId);
    }
}
