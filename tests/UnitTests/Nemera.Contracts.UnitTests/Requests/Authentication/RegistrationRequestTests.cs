#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
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
    private readonly RegistrationRequestFixture _registrationRequestFixture = new();

    [Fact]
    public void Constructor_WhenOmittingUse2fa_ShouldDefaultToTrue()
    {
        // Act
        RegistrationRequest sut = _registrationRequestFixture.Create(username: "user1", password: "pass1", passwordConfirm: "pass1", use2fa: true);

        // Assert
        Assert.True(sut.Use2fa);
    }
}
