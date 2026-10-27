#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Infrastructure.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AuthorizationRoles"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationRolesTests
{
    [Fact]
    public void ADMIN_WhenRead_ShouldBeAdmin()
    {
        // The role name is written into seeded database rows and compared during authorization checks, so it must stay stable across releases.
        // Act
        string role = AuthorizationRoles.ADMIN;

        // Assert
        Assert.Equal("Admin", role);
    }
}
