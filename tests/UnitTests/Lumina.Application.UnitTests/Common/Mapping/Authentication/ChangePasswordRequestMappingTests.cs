#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.ChangePassword.Fixtures;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
using Lumina.Application.Common.Mapping.Authentication; 
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="ChangePasswordRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordRequestMappingTests
{
    private readonly ChangePasswordRequestFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordRequestMappingTests"/> class.
    /// </summary>
    public ChangePasswordRequestMappingTests()
    {
        _fixture = new();
    }

    [Fact]
    public void ToCommand_WhenMappingRequest_ShouldMapCorrectly()
    {
        // Arrange
        ChangePasswordRequest request = _fixture.CreateChangePasswordRequest();

        // Act
        ChangePasswordCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(request.Username);
        result.CurrentPassword.Should().Be(request.CurrentPassword);
        result.NewPassword.Should().Be(request.NewPassword);
        result.NewPasswordConfirm.Should().Be(request.NewPasswordConfirm);
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithNullValues_ShouldMapCorrectly()
    {
        // Arrange
        ChangePasswordRequest request = new(null, null, null, null);

        // Act
        ChangePasswordCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().BeNull();
        result.CurrentPassword.Should().BeNull();
        result.NewPassword.Should().BeNull();
        result.NewPasswordConfirm.Should().BeNull();
    }

    [Theory]
    [InlineData("user1", "oldPass", "newPass", "newPass")]
    [InlineData("", "", "", "")]
    [InlineData("testUser", null, "pass123", "pass123")]
    public void ToCommand_WhenMappingRequestWithSpecificValues_ShouldMapCorrectly(
        string? username,
        string? currentPassword,
        string? newPassword,
        string? newPasswordConfirm)
    {
        // Arrange
        ChangePasswordRequest request = new(username, currentPassword, newPassword, newPasswordConfirm);

        // Act
        ChangePasswordCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.CurrentPassword.Should().Be(currentPassword);
        result.NewPassword.Should().Be(newPassword);
        result.NewPasswordConfirm.Should().Be(newPasswordConfirm);
    }
}
