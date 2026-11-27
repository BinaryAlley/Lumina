#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.Fixtures.Core.Requests.Authorization;
using Nemera.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleRequestTests
{
    private readonly GetUserRoleRequestFixture _getUserRoleRequestFixture = new();

    [Fact]
    public void Constructor_WhenPassingNullUserId_ShouldReturnNullUserId()
    {
        // Act
        GetUserRoleRequest sut = _getUserRoleRequestFixture.Create(includeUserId: false);

        // Assert
        Assert.Null(sut.UserId);
    }
}
