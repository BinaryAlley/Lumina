#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleRequestTests
{
    [Fact]
    public void Constructor_WhenPassingNullUserId_ShouldReturnNullUserId()
    {
        // Act
        GetUserRoleRequest sut = new(UserId: null);

        // Assert
        Assert.Null(sut.UserId);
    }
}
