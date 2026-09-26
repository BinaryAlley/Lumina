#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AuthorizationPermission"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationPermissionTests
{
    [Fact]
    public void AuthorizationPermission_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        AuthorizationPermission[] values = Enum.GetValues<AuthorizationPermission>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void None_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)AuthorizationPermission.None;

        // Assert
        Assert.Equal(0, value);
    }
}
