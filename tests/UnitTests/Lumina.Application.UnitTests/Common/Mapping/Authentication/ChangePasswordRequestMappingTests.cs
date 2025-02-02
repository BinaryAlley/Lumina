#region ========================================================================= USING =====================================================================================
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
        ChangePasswordRequest request = new(null, null, null, null);

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
        ChangePasswordRequest request = new(username, currentPassword, newPassword, newPasswordConfirm);

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
