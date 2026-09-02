#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Middlewares;

/// <summary>
/// Contains unit tests for the <see cref="NotFoundRedirectMiddleware"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class NotFoundRedirectMiddlewareTests
{
    private readonly ILogger<NotFoundRedirectMiddleware> _mockLogger;
    private readonly NotFoundRedirectMiddleware _sut;
    private RequestDelegate _nextDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundRedirectMiddlewareTests"/> class.
    /// </summary>
    public NotFoundRedirectMiddlewareTests()
    {
        _mockLogger = Substitute.For<ILogger<NotFoundRedirectMiddleware>>();
        _nextDelegate = context => Task.CompletedTask;
        _sut = new NotFoundRedirectMiddleware(context => _nextDelegate(context), _mockLogger);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseIs404AndResponseHasNotStarted_ShouldReexecuteRequestAgainstNotFoundPage()
    {
        // Arrange
        int invocationCount = 0;
        string? invokedPath = null;
        _nextDelegate = context =>
        {
            invocationCount++;
            invokedPath = context.Request.Path.Value;
            if (invocationCount == 1)
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };
        DefaultHttpContext httpContext = CreateHttpContext(culture: "de-DE");
        httpContext.Request.Path = "/en-us/books/123";

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(2, invocationCount);
        Assert.Equal("/de-de/not-found", invokedPath);
        Assert.Equal("/de-de/not-found", httpContext.Request.Path.Value);
        Assert.Equal("/en-us/books/123", httpContext.Items["originalPath"] as string);
        Assert.Equal(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenCultureComesFromRequestCultureFeature_ShouldUseTheFeatureCulture()
    {
        // Arrange
        int invocationCount = 0;
        string? invokedPath = null;
        _nextDelegate = context =>
        {
            invocationCount++;
            invokedPath = context.Request.Path.Value;
            if (invocationCount == 1)
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };
        DefaultHttpContext httpContext = CreateHttpContext(culture: null);
        IRequestCultureFeature requestCultureFeature = Substitute.For<IRequestCultureFeature>();
        requestCultureFeature.RequestCulture.Returns(new RequestCulture("fr-FR"));
        httpContext.Features.Set<IRequestCultureFeature>(requestCultureFeature);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(2, invocationCount);
        Assert.Equal("/fr-fr/not-found", invokedPath);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoCultureSourceIsAvailable_ShouldFallBackToDefaultCulture()
    {
        // Arrange
        int invocationCount = 0;
        string? invokedPath = null;
        _nextDelegate = context =>
        {
            invocationCount++;
            invokedPath = context.Request.Path.Value;
            if (invocationCount == 1)
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };
        DefaultHttpContext httpContext = CreateHttpContext(culture: null);

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(2, invocationCount);
        Assert.Equal("/en-us/not-found", invokedPath);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseStatusIsNot404_ShouldNotReexecuteRequest()
    {
        // Arrange
        int invocationCount = 0;
        _nextDelegate = context =>
        {
            invocationCount++;
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };
        DefaultHttpContext httpContext = CreateHttpContext(culture: "de-DE");
        httpContext.Request.Path = "/en-us/books/123";

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(1, invocationCount);
        Assert.Equal("/en-us/books/123", httpContext.Request.Path.Value);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseIs404AndResponseHasStarted_ShouldNotReexecuteRequest()
    {
        // Arrange
        int invocationCount = 0;
        _nextDelegate = context =>
        {
            invocationCount++;
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };
        DefaultHttpContext httpContext = CreateHttpContext(culture: "de-DE");
        httpContext.Request.Path = "/en-us/books/123";
        httpContext.Features.Set<IHttpResponseFeature>(new StartedHttpResponseFeature());

        // Act
        await _sut.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(1, invocationCount);
        Assert.Equal("/en-us/books/123", httpContext.Request.Path.Value);
    }

    /// <summary>
    /// Creates a <see cref="DefaultHttpContext"/> with a writable response body and an optional culture route value.
    /// </summary>
    /// <param name="culture">The culture route value, <see langword="null"/> when no route value is present.</param>
    /// <returns>The configured <see cref="DefaultHttpContext"/> instance.</returns>
    private static DefaultHttpContext CreateHttpContext(string? culture)
    {
        DefaultHttpContext httpContext = new()
        {
            Response = { Body = new MemoryStream() }
        };
        if (culture is not null)
            httpContext.Request.RouteValues["culture"] = culture;
        return httpContext;
    }

    /// <summary>
    /// Response feature that reports the response as already started, for simulating a response that cannot be replaced.
    /// </summary>
    private sealed class StartedHttpResponseFeature : IHttpResponseFeature
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        /// <summary>
        /// Gets or sets the reason phrase of the response.
        /// </summary>
        public string? ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets the response headers.
        /// </summary>
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        /// <summary>
        /// Gets or sets the response body stream.
        /// </summary>
        public Stream Body { get; set; } = Stream.Null;

        /// <summary>
        /// Gets a value indicating whether the response has already started.
        /// </summary>
        public bool HasStarted => true;

        /// <summary>
        /// Registers a callback to be invoked when the response is about to start.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">The state passed to the callback.</param>
        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        /// <summary>
        /// Registers a callback to be invoked when the response has completed.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">The state passed to the callback.</param>
        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }
    }
}
