#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AuthorizationPolicies"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationPoliciesTests
{
    [Fact]
    public void PolicyNames_WhenRead_ShouldMatchExpectedPolicyNames()
    {
        // Act & Assert
        Assert.Equal("RequireAdminRole", AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        Assert.Equal("RequireCreateLibrariesPermission", AuthorizationPolicies.REQUIRE_CREATE_LIBRARIES_PERMISSION);
        Assert.Equal("RequireInitialization", AuthorizationPolicies.REQUIRE_INITIALIZATION);
    }
}
