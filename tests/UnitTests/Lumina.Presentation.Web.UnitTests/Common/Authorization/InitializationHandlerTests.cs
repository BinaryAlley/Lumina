#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Microsoft.AspNetCore.Authorization;
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
/// Contains unit tests for the <see cref="InitializationHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InitializationHandlerTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly InitializationHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="InitializationHandlerTests"/> class.
    /// </summary>
    public InitializationHandlerTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = new InitializationHandler(_mockApiHttpClient);
    }

    [Fact]
    public async Task HandleAsync_WhenTheApiIsUnreachableDuringAPageNavigation_ShouldSucceedAndMarkTheServerUnreachable()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext();
        _mockApiHttpClient.GetAsync<InitializationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InitializationResponse>(new BrokenCircuitException()));
        AuthorizationHandlerContext context = CreateContext(httpContext);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.True(httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] is true);
    }

    [Fact]
    public async Task HandleAsync_WhenTheApiReturnsUnauthorizedDuringAPageNavigation_ShouldRethrow()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext();
        _mockApiHttpClient.GetAsync<InitializationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InitializationResponse>(new ApiException(null, HttpStatusCode.Unauthorized)));
        AuthorizationHandlerContext context = CreateContext(httpContext);

        // Act & Assert
        await Assert.ThrowsAsync<ApiException>(() => _sut.HandleAsync(context));
    }

    [Fact]
    public async Task HandleAsync_WhenTheApiReturnsAServerErrorDuringAPageNavigation_ShouldSucceedAndMarkTheServerUnreachable()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext();
        _mockApiHttpClient.GetAsync<InitializationResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InitializationResponse>(new ApiException(null, HttpStatusCode.InternalServerError)));
        AuthorizationHandlerContext context = CreateContext(httpContext);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.True(httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] is true);
    }

    /// <summary>
    /// Creates a page navigation HTTP context.
    /// </summary>
    /// <returns>The created HTTP context.</returns>
    private static DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers.Accept = "text/html";
        return httpContext;
    }

    /// <summary>
    /// Creates an authorization context that evaluates the initialization requirement.
    /// </summary>
    /// <param name="httpContext">The HTTP context of the request being authorized.</param>
    /// <returns>The created authorization context.</returns>
    private static AuthorizationHandlerContext CreateContext(HttpContext httpContext)
    {
        return new AuthorizationHandlerContext(
            [new InitializationRequirement()],
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "TestAuthentication")),
            httpContext);
    }
}
