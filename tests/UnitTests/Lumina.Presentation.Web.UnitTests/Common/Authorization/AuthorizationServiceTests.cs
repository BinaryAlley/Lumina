#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AuthorizationService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationServiceTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly AuthorizationService _sut;
    private readonly Guid _userId;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationServiceTests"/> class.
    /// </summary>
    public AuthorizationServiceTests()
    {
        _userId = Guid.NewGuid();
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, _userId.ToString())], "TestAuthentication"));
        httpContextAccessor.HttpContext.Returns(httpContext);
        _sut = new AuthorizationService(_mockApiHttpClient, httpContextAccessor);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserHasPermission_ShouldReturnTrue()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_getAuthorizationResponseFixture.Create(userId: _userId, role: "Admin", permissions: [AuthorizationPermission.CanCreateLibraries]));

        // Act
        bool result = await _sut.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserDoesNotHavePermission_ShouldReturnFalse()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_getAuthorizationResponseFixture.Create(userId: _userId, role: "Admin", permissions: [AuthorizationPermission.CanViewUsers]));

        // Act
        bool result = await _sut.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsInRoleAsync_WhenRoleMatches_ShouldReturnTrue()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_getAuthorizationResponseFixture.Create(userId: _userId, role: "Admin", permissions: []));

        // Act
        bool result = await _sut.IsInRoleAsync("Admin", CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsInRoleAsync_WhenRoleDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_getAuthorizationResponseFixture.Create(userId: _userId, role: "User", permissions: []));

        // Act
        bool result = await _sut.IsInRoleAsync("Admin", CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenCalled_ShouldRequestAuthorizationForCurrentUser()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_getAuthorizationResponseFixture.Create(userId: _userId, role: "Admin", permissions: []));

        // Act
        await _sut.HasPermissionAsync(AuthorizationPermission.CanViewUsers, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<GetAuthorizationResponse>(
            Arg.Is<string>(endpoint => endpoint.StartsWith($"auth/get-authorization?userId={_userId}", StringComparison.OrdinalIgnoreCase)),
            Arg.Any<CancellationToken>());
    }
}
