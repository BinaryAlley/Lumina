#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IAuthorizationService = Lumina.Presentation.Web.Common.Authorization.IAuthorizationService;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="PermissionAuthorizationHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionAuthorizationHandlerTests
{
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly DefaultHttpContext _httpContext;
    private readonly PermissionAuthorizationHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizationHandlerTests"/> class.
    /// </summary>
    public PermissionAuthorizationHandlerTests()
    {
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "TestAuthentication"))
        };
        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(_httpContext);
        _sut = new PermissionAuthorizationHandler(_mockAuthorizationService, httpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenTheUserHoldsTheRequiredPermission_ShouldSucceed()
    {
        // Arrange
        _mockAuthorizationService.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>()).Returns(true);
        AuthorizationHandlerContext context = CreateContext();

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WhenAnAdministratorLacksTheRequiredPermission_ShouldSucceed()
    {
        // Arrange
        // The API lets administrators do what the permission allows even without holding it, so the Web must not deny them.
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockAuthorizationService.HasPermissionAsync(Arg.Any<AuthorizationPermission>(), Arg.Any<CancellationToken>()).Returns(false);
        AuthorizationHandlerContext context = CreateContext();

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WhenAUserIsNeitherAnAdministratorNorHoldsTheRequiredPermission_ShouldNotSucceed()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _mockAuthorizationService.HasPermissionAsync(Arg.Any<AuthorizationPermission>(), Arg.Any<CancellationToken>()).Returns(false);
        AuthorizationHandlerContext context = CreateContext();

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WhenTheApiWasUnreachableDuringAnAuthenticatedPageNavigation_ShouldSucceed()
    {
        // Arrange
        // The authorization service returns no role or permission when the API is unreachable, and marks the request instead.
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _mockAuthorizationService.HasPermissionAsync(Arg.Any<AuthorizationPermission>(), Arg.Any<CancellationToken>()).Returns(false);
        _httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] = true;
        AuthorizationHandlerContext context = CreateContext();

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WhenTheApiWasUnreachableDuringAnAnonymousPageNavigation_ShouldNotSucceed()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _mockAuthorizationService.HasPermissionAsync(Arg.Any<AuthorizationPermission>(), Arg.Any<CancellationToken>()).Returns(false);
        _httpContext.User = new ClaimsPrincipal();
        _httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] = true;
        AuthorizationHandlerContext context = CreateContext();

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    /// <summary>
    /// Creates an authorization context that evaluates a requirement for the permission to manage media libraries.
    /// </summary>
    /// <returns>The created authorization context.</returns>
    private static AuthorizationHandlerContext CreateContext()
    {
        // Endpoint routing passes the endpoint, not the HttpContext, as the resource.
        return new AuthorizationHandlerContext(
            [new PermissionRequirement(AuthorizationPermission.CanCreateLibraries)],
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "TestAuthentication")),
            new Endpoint(null, EndpointMetadataCollection.Empty, "test"));
    }
}
