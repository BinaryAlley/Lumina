#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="RegistrationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationRequestTests
{
    [Fact]
    public void Constructor_WhenOmittingUse2fa_ShouldDefaultToTrue()
    {
        // Act
        RegistrationRequest sut = new(Username: "user1", Password: "pass1", PasswordConfirm: "pass1");

        // Assert
        Assert.True(sut.Use2fa);
    }
}
