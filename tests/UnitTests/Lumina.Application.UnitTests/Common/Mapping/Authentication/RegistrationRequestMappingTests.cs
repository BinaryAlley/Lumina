#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RegisterUser.Fixture;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="RegistrationRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationRequestMappingTests
{
    private readonly RegistrationRequestFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationRequestMappingTests"/> class.
    /// </summary>
    public RegistrationRequestMappingTests()
    {
        _fixture = new();
    }

    [Fact]
    public void ToSetupCommand_WhenMappingRequest_ShouldMapCorrectly()
    {
        // Arrange
        RegistrationRequest request = _fixture.CreateRegistrationRequest();

        // Act
        SetupApplicationCommand result = request.ToSetupCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(request.Username);
        result.Password.Should().Be(request.Password);
        result.PasswordConfirm.Should().Be(request.PasswordConfirm);
        result.Use2fa.Should().Be(request.Use2fa);
    }

    [Fact]
    public void ToCommand_WhenMappingRequest_ShouldMapCorrectly()
    {
        // Arrange
        RegistrationRequest request = _fixture.CreateRegistrationRequest();

        // Act
        RegisterUserCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(request.Username);
        result.Password.Should().Be(request.Password);
        result.PasswordConfirm.Should().Be(request.PasswordConfirm);
        result.Use2fa.Should().Be(request.Use2fa);
    }

    [Fact]
    public void ToSetupCommand_WhenMappingRequestWithNullValues_ShouldMapCorrectly()
    {
        // Arrange
        RegistrationRequest request = new();

        // Act
        SetupApplicationCommand result = request.ToSetupCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().BeNull();
        result.Password.Should().BeNull();
        result.PasswordConfirm.Should().BeNull();
        result.Use2fa.Should().BeTrue();
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithNullValues_ShouldMapCorrectly()
    {
        // Arrange
        RegistrationRequest request = new();

        // Act
        RegisterUserCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().BeNull();
        result.Password.Should().BeNull();
        result.PasswordConfirm.Should().BeNull();
        result.Use2fa.Should().BeTrue();
    }

    [Theory]
    [InlineData("user1", "pass123", "pass123", true)]
    [InlineData("", "", "", false)]
    [InlineData("testUser", null, null, true)]
    [InlineData(null, "pass123", "pass123", false)]
    public void ToSetupCommand_WhenMappingRequestWithSpecificValues_ShouldMapCorrectly(
        string? username,
        string? password,
        string? passwordConfirm,
        bool use2fa)
    {
        // Arrange
        RegistrationRequest request = new(username, password, passwordConfirm, use2fa);

        // Act
        SetupApplicationCommand result = request.ToSetupCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.Password.Should().Be(password);
        result.PasswordConfirm.Should().Be(passwordConfirm);
        result.Use2fa.Should().Be(use2fa);
    }

    [Theory]
    [InlineData("user1", "pass123", "pass123", true)]
    [InlineData("", "", "", false)]
    [InlineData("testUser", null, null, true)]
    [InlineData(null, "pass123", "pass123", false)]
    public void ToCommand_WhenMappingRequestWithSpecificValues_ShouldMapCorrectly(
        string? username,
        string? password,
        string? passwordConfirm,
        bool use2fa)
    {
        // Arrange
        RegistrationRequest request = new(username, password, passwordConfirm, use2fa);

        // Act
        RegisterUserCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.Password.Should().Be(password);
        result.PasswordConfirm.Should().Be(passwordConfirm);
        result.Use2fa.Should().Be(use2fa);
    }
}
