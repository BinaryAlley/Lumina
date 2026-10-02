#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
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
    private readonly DeleteRoleRequestFixture _deleteRoleRequestFixture = new();

    [Fact]
    public void Constructor_WhenPassingNullRoleId_ShouldReturnNullRoleId()
    {
        // Act
        DeleteRoleRequest sut = _deleteRoleRequestFixture.Create(includeRoleId: false);

        // Assert
        Assert.Null(sut.RoleId);
    }
}
