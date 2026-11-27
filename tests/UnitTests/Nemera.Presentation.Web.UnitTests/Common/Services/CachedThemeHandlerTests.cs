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
/// Contains unit tests for the <see cref="CachedThemeHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CachedThemeHandlerTests
{
    private readonly HybridCache _hybridCache;
    private readonly ThemeCachePreferenceService _themeCachePreferenceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedThemeHandlerTests"/> class.
    /// </summary>
    public CachedThemeHandlerTests()
    {
        ServiceCollection services = new();
        services.AddHybridCache();
        _hybridCache = services.BuildServiceProvider().GetRequiredService<HybridCache>();
        _themeCachePreferenceService = new ThemeCachePreferenceService(_hybridCache);
    }

    [Fact]
    public async Task SendAsync_WhenThemeGetRequestIsServedTwice_ShouldCallInnerHandlerOnlyOnce()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """{"themeId":"editorial-paper"}"""));
        HttpClient client = CreateClient(messageHandler);

        // Act
        await client.GetAsync("http://localhost:5214/api/v1/themes/editorial-paper/templates/shared/layout", CancellationToken.None);
        await client.GetAsync("http://localhost:5214/api/v1/themes/editorial-paper/templates/shared/layout", CancellationToken.None);

        // Assert
        Assert.Single(messageHandler.Requests);
    }

    [Fact]
    public async Task SendAsync_WhenNonThemeGetRequestIsServedTwice_ShouldCallInnerHandlerTwice()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """[{"title":"Library One"}]"""));
        HttpClient client = CreateClient(messageHandler);

        // Act
        await client.GetAsync("http://localhost:5214/api/v1/libraries", CancellationToken.None);
        await client.GetAsync("http://localhost:5214/api/v1/libraries", CancellationToken.None);

        // Assert
        Assert.Equal(2, messageHandler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_WhenThemeMutationIsPerformed_ShouldInvalidateCachedThemeResponses()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """{"themeId":"editorial-paper"}"""));
        HttpClient client = CreateClient(messageHandler);

        // Act
        await client.GetAsync("http://localhost:5214/api/v1/themes/current", CancellationToken.None);
        await client.PutAsync("http://localhost:5214/api/v1/themes/current", new StringContent("""{"themeId":"lumina-default"}""", Encoding.UTF8, "application/json"), CancellationToken.None);
        await client.GetAsync("http://localhost:5214/api/v1/themes/current", CancellationToken.None);

        // Assert
        Assert.Equal(3, messageHandler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_WhenThemeGetRequestFails_ShouldNotRetainTheFailedResponse()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        HttpClient client = CreateClient(messageHandler);

        // Act
        await client.GetAsync("http://localhost:5214/api/v1/themes/current", CancellationToken.None);
        await client.GetAsync("http://localhost:5214/api/v1/themes/current", CancellationToken.None);

        // Assert
        Assert.Equal(2, messageHandler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_WhenThemeGetResponseExceedsMaxCacheableSize_ShouldNotCacheIt()
    {
        // Arrange
        byte[] largeContent = new byte[600 * 1024];
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(largeContent) });
        HttpClient client = CreateClient(messageHandler);

        // Act
        await client.GetAsync("http://localhost:5214/api/v1/themes/editorial-paper/assets/preview.png", CancellationToken.None);
        await client.GetAsync("http://localhost:5214/api/v1/themes/editorial-paper/assets/preview.png", CancellationToken.None);

        // Assert
        Assert.Equal(2, messageHandler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_WhenUserDisablesThemeCaching_ShouldCallInnerHandlerForEveryRequest()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TestApiHttpMessageHandler messageHandler = new(_ => CreateJsonResponse(HttpStatusCode.OK, """{"themeId":"editorial-paper"}"""));
        HttpClient client = CreateClient(messageHandler, userId: userId);
        await _themeCachePreferenceService.SetAsync(userId, isEnabled: false, CancellationToken.None);

        // Act
        await client.GetAsync("http://localhost:5214/api/v1/themes/current", CancellationToken.None);
        await client.GetAsync("http://localhost:5214/api/v1/themes/current", CancellationToken.None);

        // Assert
        Assert.Equal(2, messageHandler.Requests.Count);
    }

    private HttpClient CreateClient(TestApiHttpMessageHandler messageHandler, Guid? userId = null)
    {
        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        if (userId is not null)
        {
            DefaultHttpContext httpContext = new();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())], "TestAuthentication"));
            httpContextAccessor.HttpContext.Returns(httpContext);
        }
        CachedThemeHandler themeHandler = new(_hybridCache, httpContextAccessor, _themeCachePreferenceService)
        {
            InnerHandler = messageHandler
        };
        return new HttpClient(themeHandler);
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}
