#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="RegistrationRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationRequestTests
{
    private readonly RegistrationRequestFixture _registrationRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidRegistrationRequest()
    {
        // Act
        RegistrationRequest sut = _registrationRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Username));
        Assert.False(string.IsNullOrWhiteSpace(sut.Password));
        Assert.Equal(sut.Password, sut.PasswordConfirm);
    }

    [Fact]
    public void Constructor_WhenOmittingUse2fa_ShouldDefaultToTrue()
    {
        // Act
        RegistrationRequest sut = new(Username: "user1", Password: "pass1", PasswordConfirm: "pass1");

        // Assert
        Assert.True(sut.Use2fa);
    }

    [Fact]
    public void RoundTrip_WhenSerializingRegistrationRequest_ShouldPreserveValues()
    {
        // Arrange
        RegistrationRequest expected = _registrationRequestFixture.Create(use2fa: true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RegistrationRequest? actual = JsonSerializer.Deserialize<RegistrationRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        RegistrationRequest sut = _registrationRequestFixture.Create(username: "user1", password: "pass1", passwordConfirm: "pass1", use2fa: false);

        // Act
        (string? username, string? password, string? passwordConfirm, bool use2fa) = sut;

        // Assert
        Assert.Equal(sut.Username, username);
        Assert.Equal(sut.Password, password);
        Assert.Equal(sut.PasswordConfirm, passwordConfirm);
        Assert.Equal(sut.Use2fa, use2fa);
    }
}
