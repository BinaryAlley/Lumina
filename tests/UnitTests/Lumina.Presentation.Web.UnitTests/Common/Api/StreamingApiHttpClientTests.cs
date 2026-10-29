#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Fixtures.Common.Api;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Api;

/// <summary>
/// Contains unit tests for the <see cref="StreamingApiHttpClient"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamingApiHttpClientTests
{
    private const string ENDPOINT = "libraries/tracks/1/stream";

    private readonly ServerConfigurationDtoFixture _serverConfigurationDtoFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldConfigureTheBaseAddressFromTheServerConfiguration()
    {
        // Arrange
        HttpClient httpClient = new(new TestApiHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));

        // Act
        _ = CreateSut(httpClient);

        // Assert
        Assert.Equal(new Uri("http://localhost:5214/api/v1/"), httpClient.BaseAddress);
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldConfigureATwelveHourTimeout()
    {
        // Arrange
        HttpClient httpClient = new(new TestApiHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));

        // Act
        _ = CreateSut(httpClient);

        // Assert
        Assert.Equal(TimeSpan.FromHours(12), httpClient.Timeout);
    }

    [Fact]
    public async Task GetStreamAsync_WhenCalled_ShouldSendAGetRequestToTheEndpoint()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateStreamResponse(HttpStatusCode.OK));
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        await using ApiStreamResult result = await sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpMethod.Get, messageHandler.Requests[0].Method);
        Assert.Equal(new Uri("http://localhost:5214/api/v1/libraries/tracks/1/stream"), messageHandler.Requests[0].RequestUri);
    }

    [Fact]
    public async Task GetStreamAsync_WhenRequestHeadersProvided_ShouldForwardThemVerbatim()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateStreamResponse(HttpStatusCode.OK));
        StreamingApiHttpClient sut = CreateSut(messageHandler);
        Dictionary<string, string> requestHeaders = new()
        {
            ["Range"] = "bytes=1024-2047",
            ["If-None-Match"] = "\"etag-value\"",
            ["If-Modified-Since"] = "Wed, 21 Oct 2026 07:28:00 GMT"
        };

        // Act
        await using ApiStreamResult result = await sut.GetStreamAsync(ENDPOINT, requestHeaders, CancellationToken.None);

        // Assert
        HttpRequestMessage request = messageHandler.Requests[0];
        Assert.Equal("bytes=1024-2047", request.Headers.Range!.ToString());
        Assert.Equal("\"etag-value\"", Assert.Single(request.Headers.IfNoneMatch).Tag);
        Assert.Equal(new DateTimeOffset(2026, 10, 21, 7, 28, 0, TimeSpan.Zero), request.Headers.IfModifiedSince);
    }

    [Fact]
    public async Task GetStreamAsync_WhenUserHasToken_ShouldSendTheBearerToken()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateStreamResponse(HttpStatusCode.OK));
        StreamingApiHttpClient sut = CreateSut(messageHandler, token: "test-jwt-token");

        // Act
        await using ApiStreamResult result = await sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None);

        // Assert
        Assert.Equal("Bearer", messageHandler.Requests[0].Headers.Authorization!.Scheme);
        Assert.Equal("test-jwt-token", messageHandler.Requests[0].Headers.Authorization!.Parameter);
    }

    [Theory]
    [InlineData(false, null)] // there is no HTTP context at all
    [InlineData(true, null)] // the HTTP context has no token claim
    [InlineData(true, "")] // the token claim is empty
    public async Task GetStreamAsync_WhenTokenIsUnavailable_ShouldNotSendAnAuthorizationHeader(bool hasHttpContext, string? token)
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => CreateStreamResponse(HttpStatusCode.OK));
        StreamingApiHttpClient sut = CreateSut(messageHandler, token: token, hasHttpContext: hasHttpContext);

        // Act
        await using ApiStreamResult result = await sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None);

        // Assert
        Assert.Null(messageHandler.Requests[0].Headers.Authorization);
    }

    [Fact]
    public async Task GetStreamAsync_WhenCalled_ShouldHandOverTheContentStreamWithoutBufferingIt()
    {
        // Arrange
        TrackingStreamContent content = new(Encoding.UTF8.GetBytes("streamed-audio"), "audio/mp4");
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        await using ApiStreamResult result = await sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None);

        // Assert
        Assert.False(content.WasBufferedByClient);
        Assert.Same(content.ContentStream, result.Content);
    }

    [Fact]
    public async Task GetStreamAsync_WhenApiReturnsOk_ShouldReturnAStreamResultWithTheContent()
    {
        // Arrange
        byte[] contentBytes = Encoding.UTF8.GetBytes("streamed-audio");
        TrackingStreamContent content = new(contentBytes, "audio/mp4");
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        await using ApiStreamResult result = await sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("audio/mp4", result.ContentType);
        Assert.Equal(contentBytes.Length, result.ContentLength);
    }

    [Fact]
    public async Task GetStreamAsync_WhenApiReturnsPartialContent_ShouldReturnAStreamResultWithTheRangeHeaders()
    {
        // Arrange
        byte[] contentBytes = Encoding.UTF8.GetBytes("streamed-audio-segment");
        TrackingStreamContent content = new(contentBytes, "audio/mp4");
        content.Headers.ContentRange = new ContentRangeHeaderValue(0, contentBytes.Length - 1, 4096);
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.PartialContent) { Content = content });
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        await using ApiStreamResult result = await sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.PartialContent, result.StatusCode);
        Assert.Equal($"bytes 0-{contentBytes.Length - 1}/4096", result.ContentRange);
        Assert.Equal(contentBytes.Length, result.ContentLength);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotModified)] // a conditional request that the entity tag did not invalidate
    [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)] // a ranged request that cannot be satisfied
    public async Task GetStreamAsync_WhenApiReturnsANonSuccessThatIsAValidOutcome_ShouldPassTheResponseThrough(HttpStatusCode statusCode)
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(statusCode));
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        await using ApiStreamResult result = await sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None);

        // Assert
        Assert.Equal(statusCode, result.StatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)] // a malformed request
    [InlineData(HttpStatusCode.NotFound)] // the track does not exist
    [InlineData(HttpStatusCode.InternalServerError)] // the API failed
    public async Task GetStreamAsync_WhenApiReturnsAnotherNonSuccess_ShouldThrowApiExceptionWithTheProblemDetails(HttpStatusCode statusCode)
    {
        // Arrange
        string problemDetailsJson = $$"""
        {
            "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            "title": "General.NotFound",
            "status": {{(int)statusCode}},
            "detail": "The requested track was not found."
        }
        """;
        TrackingStreamContent content = new(Encoding.UTF8.GetBytes(problemDetailsJson), "application/problem+json");
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(statusCode) { Content = content });
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None));

        // Assert
        Assert.Equal(statusCode, exception.HttpStatusCode);
        Assert.NotNull(exception.ProblemDetails);
        Assert.Equal("General.NotFound", exception.ProblemDetails!.Title);
        Assert.Equal((int)statusCode, exception.ProblemDetails!.Status);
        Assert.Equal("The requested track was not found.", exception.ProblemDetails!.Detail);
        Assert.Equal("/api/v1/libraries/tracks/1/stream", exception.RequestPath);
    }

    [Fact]
    public async Task GetStreamAsync_WhenErrorBodyIsNotProblemDetails_ShouldThrowApiExceptionWithoutProblemDetails()
    {
        // Arrange
        TrackingStreamContent content = new(Encoding.UTF8.GetBytes("<html>upstream failure</html>"), "text/html");
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = content });
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None));

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, exception.HttpStatusCode);
        Assert.Null(exception.ProblemDetails);
        Assert.Equal("An error occurred.", exception.Message);
    }

    [Fact]
    public async Task GetStreamAsync_WhenApiReturnsANonSuccess_ShouldDisposeTheResponse()
    {
        // Arrange
        TrackingStreamContent content = new(Encoding.UTF8.GetBytes("""{"title":"General.Failure","status":500}"""), "application/json");
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = content });
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        await Assert.ThrowsAsync<ApiException>(() => sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None));

        // Assert
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public async Task GetStreamAsync_WhenContentStreamCannotBeObtained_ShouldDisposeTheResponse()
    {
        // Arrange
        TrackingStreamContent content = new(Encoding.UTF8.GetBytes("streamed-audio"), "audio/mp4", shouldFailStreamCreation: true);
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), CancellationToken.None));

        // Assert
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public async Task GetStreamAsync_WhenCancellationIsRequested_ShouldCancelTheOperation()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> requestStarted = new();
        TestApiHttpMessageHandler messageHandler = new(async (request, cancellationToken) =>
        {
            requestStarted.SetResult(true);
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        StreamingApiHttpClient sut = CreateSut(messageHandler);

        // Act
        Task<ApiStreamResult> operationTask = sut.GetStreamAsync(ENDPOINT, CreateForwardedHeaders(), cancellationTokenSource.Token);
        await requestStarted.Task;
        cancellationTokenSource.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
        Assert.True(messageHandler.CancellationTokens[0].IsCancellationRequested);
    }

    /// <summary>
    /// Creates the system under test around the provided HTTP client.
    /// </summary>
    /// <param name="httpClient">The HTTP client the system under test sends its requests through.</param>
    /// <returns>The created <see cref="StreamingApiHttpClient"/>.</returns>
    private StreamingApiHttpClient CreateSut(HttpClient httpClient)
    {
        IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions = Substitute.For<IOptionsSnapshot<ServerConfigurationDto>>();
        serverConfigurationOptions.Value.Returns(_serverConfigurationDtoFixture.Create(apiVersion: '1', baseAddress: "http://localhost", port: 5214));
        return new StreamingApiHttpClient(httpClient, Substitute.For<IHttpContextAccessor>(), serverConfigurationOptions);
    }

    /// <summary>
    /// Creates the system under test configured with the provided message handler.
    /// </summary>
    /// <param name="messageHandler">The message handler backing the inner <see cref="HttpClient"/>.</param>
    /// <param name="token">Optional token claim to place on the current HTTP context user.</param>
    /// <param name="hasHttpContext">Whether an HTTP context is available to the system under test.</param>
    /// <returns>The created <see cref="StreamingApiHttpClient"/>.</returns>
    private StreamingApiHttpClient CreateSut(TestApiHttpMessageHandler messageHandler, string? token = null, bool hasHttpContext = true)
    {
        HttpClient httpClient = new(messageHandler);
        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        if (hasHttpContext)
        {
            Claim[] claims = token is null ? [] : [new Claim("Token", token)];
            DefaultHttpContext httpContext = new()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"))
            };
            httpContextAccessor.HttpContext.Returns(httpContext);
        }
        return CreateSut(httpClient, httpContextAccessor);
    }

    /// <summary>
    /// Creates the system under test around the provided HTTP client and HTTP context accessor.
    /// </summary>
    /// <param name="httpClient">The HTTP client the system under test sends its requests through.</param>
    /// <param name="httpContextAccessor">The accessor that provides the current HTTP context.</param>
    /// <returns>The created <see cref="StreamingApiHttpClient"/>.</returns>
    private StreamingApiHttpClient CreateSut(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions = Substitute.For<IOptionsSnapshot<ServerConfigurationDto>>();
        serverConfigurationOptions.Value.Returns(_serverConfigurationDtoFixture.Create(apiVersion: '1', baseAddress: "http://localhost", port: 5214));
        return new StreamingApiHttpClient(httpClient, httpContextAccessor, serverConfigurationOptions);
    }

    /// <summary>
    /// Creates the range and conditional headers an incoming streaming request forwards to the API.
    /// </summary>
    /// <returns>The forwarded headers.</returns>
    private static Dictionary<string, string> CreateForwardedHeaders()
    {
        return new Dictionary<string, string>
        {
            ["Range"] = "bytes=0-1023",
            ["If-None-Match"] = "\"etag-value\""
        };
    }

    /// <summary>
    /// Creates a streamed API response with the given status code and a known content stream.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <returns>The created HTTP response.</returns>
    private static HttpResponseMessage CreateStreamResponse(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new TrackingStreamContent(Encoding.UTF8.GetBytes("streamed-audio"), "audio/mp4")
        };
    }
}
