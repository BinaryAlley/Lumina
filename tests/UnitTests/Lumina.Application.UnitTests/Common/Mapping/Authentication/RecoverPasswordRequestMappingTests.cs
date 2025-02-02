#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Commands.RecoverPassword.Fixtures;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="RecoverPasswordRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordRequestMappingTests
{
    private readonly RecoverPasswordRequestFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordRequestMappingTests"/> class.
    /// </summary>
    public RecoverPasswordRequestMappingTests()
    {
        _fixture = new();
    }

    [Fact]
    public void ToCommand_WhenMappingRequest_ShouldMapCorrectly()
    {
        // Arrange
        RecoverPasswordRequest request = _fixture.CreateRecoverPasswordRequest();

        // Act
        RecoverPasswordCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Username, result.Username);
        Assert.Equal(request.TotpCode, result.TotpCode);
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithNullValues_ShouldMapCorrectly()
    {
        // Arrange
        RecoverPasswordRequest request = new(null, null);

        // Act
        RecoverPasswordCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Username);
        Assert.Null(result.TotpCode);
    }

    [Theory]
    [InlineData("user1", "123456")]
    [InlineData("", "")]
    [InlineData("testUser", null)]
    [InlineData(null, "654321")]
    public void ToCommand_WhenMappingRequestWithSpecificValues_ShouldMapCorrectly(
        string? username,
        string? totpCode)
    {
        // Arrange
        RecoverPasswordRequest request = new(username, totpCode);

        // Act
        RecoverPasswordCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal(totpCode, result.TotpCode);
    }
}
