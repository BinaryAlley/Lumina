#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.Fixtures.Core.Requests.Authorization;
using Nemera.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationRequestTests
{
    private readonly GetAuthorizationRequestFixture _getAuthorizationRequestFixture = new();

    [Fact]
    public void Constructor_WhenPassingNullUserId_ShouldReturnNullUserId()
    {
        // Act
        GetAuthorizationRequest sut = _getAuthorizationRequestFixture.Create(includeUserId: false);

        // Assert
        Assert.Null(sut.UserId);
    }
}
