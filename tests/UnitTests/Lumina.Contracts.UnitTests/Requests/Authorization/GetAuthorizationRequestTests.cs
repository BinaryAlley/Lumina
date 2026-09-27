#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationRequestTests
{
    [Fact]
    public void Constructor_WhenPassingNullUserId_ShouldReturnNullUserId()
    {
        // Act
        GetAuthorizationRequest sut = new(UserId: null);

        // Assert
        Assert.Null(sut.UserId);
    }
}
