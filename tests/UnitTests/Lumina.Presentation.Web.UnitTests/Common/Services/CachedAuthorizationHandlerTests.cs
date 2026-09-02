#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Fixtures.Common.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Services;

/// <summary>
/// Contains unit tests for the <see cref="CachedAuthorizationHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CachedAuthorizationHandlerTests
{
    private const string AUTHORIZATION_ENDPOINT = "http://localhost:5214/api/v1/auth/get-authorization";
    private const string LOGIN_ENDPOINT = "http://localhost:5214/api/v1/auth/login";
    private readonly HybridCache _hybridCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedAuthorizationHandlerTests"/> class.
    /// </summary>
    public CachedAuthorizationHandlerTests()
    {
        ServiceCollection services = new();
        services.AddHybridCache();
        _hybridCache = services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    [Fact]
    public async Task SendAsync_WhenAuthorizationRequestIsServedTwice_ShouldCallInnerHandlerOnlyOnce()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """{"role":"Admin","permissions":[]}"""));
        HttpClient client = CreateClient(userId: Guid.NewGuid(), messageHandler);

        // Act
        HttpResponseMessage firstResponse = await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);
        HttpResponseMessage secondResponse = await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);

        // Assert
        Assert.Single(messageHandler.Requests);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.Equal(await firstResponse.Content.ReadAsStringAsync(), await secondResponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SendAsync_WhenNonAuthorizationRequestIsServedTwice_ShouldCallInnerHandlerTwice()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """[{"title":"Library One"}]"""));
        HttpClient client = CreateClient(userId: Guid.NewGuid(), messageHandler);

        // Act
        await client.GetAsync("http://localhost:5214/api/v1/libraries", CancellationToken.None);
        await client.GetAsync("http://localhost:5214/api/v1/libraries", CancellationToken.None);

        // Assert
        Assert.Equal(2, messageHandler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_WhenLoginRequestFollowsCachedAuthorizationRequest_ShouldInvalidateCachedAuthorization()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """{"role":"Admin","permissions":[]}"""));
        HttpClient client = CreateClient(userId: Guid.NewGuid(), messageHandler);

        // Act
        await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);
        await client.PostAsync(LOGIN_ENDPOINT, new StringContent("""{"username":"testuser","password":"TestPass123!"}""", Encoding.UTF8, "application/json"), CancellationToken.None);
        await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);

        // Assert
        Assert.Equal(3, messageHandler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_WhenAuthorizationReturnsUnauthorized_ShouldNotRetainTheCachedResponse()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.Unauthorized, """{"title":"Unauthorized"}"""));
        HttpClient client = CreateClient(userId: Guid.NewGuid(), messageHandler);

        // Act
        HttpResponseMessage firstResponse = await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);
        HttpResponseMessage secondResponse = await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);

        // Assert
        Assert.Equal(2, messageHandler.Requests.Count);
        Assert.Equal(HttpStatusCode.Unauthorized, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
    }

    [Fact]
    public async Task SendAsync_WhenAuthorizationReturnsNotFound_ShouldNotRetainTheCachedResponse()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.NotFound, """{"title":"NotFound"}"""));
        HttpClient client = CreateClient(userId: Guid.NewGuid(), messageHandler);

        // Act
        await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);
        await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);

        // Assert
        Assert.Equal(2, messageHandler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_WhenAuthorizationRequestIsMadeWithoutAuthenticatedUser_ShouldStillCacheTheResponse()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """{"role":"Admin","permissions":[]}"""));
        HttpClient client = CreateClient(userId: null, messageHandler);

        // Act
        await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);
        await client.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);

        // Assert
        Assert.Single(messageHandler.Requests);
    }

    [Fact]
    public async Task SendAsync_WhenDifferentUsersRequestAuthorization_ShouldServeSeparateCachedResponses()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """{"role":"Admin","permissions":[]}"""));
        HttpClient firstClient = CreateClient(userId: Guid.NewGuid(), messageHandler);
        HttpClient secondClient = CreateClient(userId: Guid.NewGuid(), messageHandler);

        // Act
        await firstClient.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);
        await firstClient.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);

        // Assert
        Assert.Equal(1, messageHandler.Requests.Count);

        // Act
        await secondClient.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);
        await secondClient.GetAsync(AUTHORIZATION_ENDPOINT, CancellationToken.None);

        // Assert
        Assert.Equal(2, messageHandler.Requests.Count);
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> that pipes requests through a <see cref="CachedAuthorizationHandler"/> backed by the given inner handler.
    /// </summary>
    /// <param name="userId">The user id claim placed on the current HTTP context user, <see langword="null"/> for anonymous requests.</param>
    /// <param name="messageHandler">The message handler backing the inner <see cref="HttpClient"/> pipeline.</param>
    /// <returns>The created <see cref="HttpClient"/>.</returns>
    private HttpClient CreateClient(Guid? userId, TestApiHttpMessageHandler messageHandler)
    {
        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        DefaultHttpContext httpContext = new();
        if (userId is not null)
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())], "TestAuthentication"));
        httpContextAccessor.HttpContext.Returns(httpContext);
        CachedAuthorizationHandler authorizationHandler = new(_hybridCache, httpContextAccessor)
        {
            InnerHandler = messageHandler
        };
        return new HttpClient(authorizationHandler);
    }

    /// <summary>
    /// Creates an <see cref="HttpResponseMessage"/> with the given status code and JSON body.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="json">The JSON payload of the response body.</param>
    /// <returns>The created <see cref="HttpResponseMessage"/>.</returns>
    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}
