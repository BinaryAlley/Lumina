#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Authentication; 
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="ChangePasswordRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordRequestMappingTests
{
    private readonly ChangePasswordRequestFixture _changePasswordRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingRequest_ShouldMapCorrectly()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();

        // Act
        ChangePasswordCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Username, result.Username);
        Assert.Equal(request.CurrentPassword, result.CurrentPassword);
        Assert.Equal(request.NewPassword, result.NewPassword);
        Assert.Equal(request.NewPasswordConfirm, result.NewPasswordConfirm);
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithNullValues_ShouldMapCorrectly()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create(includeUsername: false, includeCurrentPassword: false, includeNewPassword: false, includeNewPasswordConfirm: false);

        // Act
        ChangePasswordCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Username);
        Assert.Null(result.CurrentPassword);
        Assert.Null(result.NewPassword);
        Assert.Null(result.NewPasswordConfirm);
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
        ChangePasswordRequest request = _changePasswordRequestFixture.Create(
            username,
            currentPassword,
            newPassword,
            newPasswordConfirm,
            includeUsername: username is not null,
            includeCurrentPassword: currentPassword is not null,
            includeNewPassword: newPassword is not null,
            includeNewPasswordConfirm: newPasswordConfirm is not null);

        // Act
        ChangePasswordCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal(currentPassword, result.CurrentPassword);
        Assert.Equal(newPassword, result.NewPassword);
        Assert.Equal(newPasswordConfirm, result.NewPasswordConfirm);
    }
}
