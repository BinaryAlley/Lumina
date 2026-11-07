#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Polly.CircuitBreaker;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
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
    private readonly DefaultHttpContext _httpContext;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationServiceTests"/> class.
    /// </summary>
    public AuthorizationServiceTests()
    {
        _userId = Guid.NewGuid();
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContext = new DefaultHttpContext();
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, _userId.ToString())], "TestAuthentication"));
        httpContextAccessor.HttpContext.Returns(_httpContext);
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

    [Fact]
    public async Task HasPermissionAsync_WhenTheApiIsUnreachableDuringAPageNavigation_ShouldReturnFalseAndMarkTheServerUnreachable()
    {
        // Arrange
        _httpContext.Request.Headers.Accept = "text/html";
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GetAuthorizationResponse>(new BrokenCircuitException()));

        // Act
        bool result = await _sut.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.True(_httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] is true);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenTheApiIsUnreachableDuringAJsonRequest_ShouldRethrow()
    {
        // Arrange
        _httpContext.Request.Headers.Accept = "application/json";
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GetAuthorizationResponse>(new BrokenCircuitException()));

        // Act & Assert
        await Assert.ThrowsAsync<BrokenCircuitException>(() => _sut.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, CancellationToken.None));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task HasPermissionAsync_WhenTheApiReturnsAnAuthorizationStatusDuringAPageNavigation_ShouldRethrow(HttpStatusCode statusCode)
    {
        // Arrange
        _httpContext.Request.Headers.Accept = "text/html";
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GetAuthorizationResponse>(new ApiException(null, statusCode)));

        // Act & Assert
        await Assert.ThrowsAsync<ApiException>(() => _sut.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, CancellationToken.None));
    }

    [Fact]
    public async Task HasPermissionAsync_WhenTheApiReturnsAServerErrorDuringAPageNavigation_ShouldReturnFalseAndMarkTheServerUnreachable()
    {
        // Arrange
        _httpContext.Request.Headers.Accept = "text/html";
        _mockApiHttpClient.GetAsync<GetAuthorizationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GetAuthorizationResponse>(new ApiException(null, HttpStatusCode.InternalServerError)));

        // Act
        bool result = await _sut.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.True(_httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] is true);
    }
}
