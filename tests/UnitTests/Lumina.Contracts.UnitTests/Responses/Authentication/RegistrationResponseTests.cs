#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.Authentication;
using Lumina.Contracts.Responses.Authentication;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="RegistrationResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationResponseTests
{
    private readonly RegistrationResponseFixture _registrationResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingRegistrationResponse_ShouldPreserveValues()
    {
        // Arrange
        RegistrationResponse expected = _registrationResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RegistrationResponse? actual = JsonSerializer.Deserialize<RegistrationResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingRegistrationResponseWithNullTotpSecret_ShouldPreserveNull()
    {
        // Arrange
        RegistrationResponse expected = _registrationResponseFixture.Create() with { TotpSecret = null };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RegistrationResponse? actual = JsonSerializer.Deserialize<RegistrationResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.TotpSecret);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        RegistrationResponse sut = _registrationResponseFixture.Create();

        // Act
        (Guid sutId, string username, string? totpSecret) = sut;

        // Assert
        Assert.Equal(sut.Id, sutId);
        Assert.Equal(sut.Username, username);
        Assert.Equal(sut.TotpSecret, totpSecret);
    }
}
