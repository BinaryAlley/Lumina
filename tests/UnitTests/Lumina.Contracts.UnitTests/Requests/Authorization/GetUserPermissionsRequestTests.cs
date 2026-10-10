#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetUserPermissionsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsRequestTests
{
    private readonly GetUserPermissionsRequestFixture _getUserPermissionsRequestFixture = new();

    [Fact]
    public void Constructor_WhenPassingNullUserId_ShouldReturnNullUserId()
    {
        // Act
        GetUserPermissionsRequest sut = _getUserPermissionsRequestFixture.Create(includeUserId: false);

        // Assert
        Assert.Null(sut.UserId);
    }
}
