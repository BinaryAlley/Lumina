#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Middlewares;

/// <summary>
/// Contains unit tests for the <see cref="CacheControlHeaderMiddleware"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CacheControlHeaderMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenTheEndpointStoredACacheControlValue_ShouldRestoreItAfterLaterCallbacksRun()
    {
        // Arrange
        DefaultHttpContext context = new();
        TestHttpResponseFeature responseFeature = new();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Items[HttpContextItemKeys.CACHE_CONTROL] = "private, no-store";
        CacheControlHeaderMiddleware sut = new(nextContext =>
        {
            // The session middleware registers its cache header write after this middleware, so it runs first when the response starts.
            nextContext.Response.OnStarting(() =>
            {
                nextContext.Response.Headers.CacheControl = "no-cache,no-store";
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        });

        // Act
        await sut.InvokeAsync(context);
        await responseFeature.StartAsync();

        // Assert
        Assert.Equal("private, no-store", context.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenNoCacheControlValueWasStored_ShouldLeaveTheHeaderWrittenByLaterCallbacks()
    {
        // Arrange
        DefaultHttpContext context = new();
        TestHttpResponseFeature responseFeature = new();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        CacheControlHeaderMiddleware sut = new(nextContext =>
        {
            nextContext.Response.OnStarting(() =>
            {
                nextContext.Response.Headers.CacheControl = "no-cache,no-store";
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        });

        // Act
        await sut.InvokeAsync(context);
        await responseFeature.StartAsync();

        // Assert
        Assert.Equal("no-cache,no-store", context.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenTheStoredValueIsEmpty_ShouldLeaveTheHeaderWrittenByLaterCallbacks()
    {
        // Arrange
        DefaultHttpContext context = new();
        TestHttpResponseFeature responseFeature = new();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Items[HttpContextItemKeys.CACHE_CONTROL] = string.Empty;
        CacheControlHeaderMiddleware sut = new(nextContext =>
        {
            nextContext.Response.OnStarting(() =>
            {
                nextContext.Response.Headers.CacheControl = "no-cache,no-store";
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        });

        // Act
        await sut.InvokeAsync(context);
        await responseFeature.StartAsync();

        // Assert
        Assert.Equal("no-cache,no-store", context.Response.Headers.CacheControl.ToString());
    }

    /// <summary>
    /// Minimal response feature that stores the response-start callbacks and invokes them in reverse registration order, like the server does.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class TestHttpResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _onStartingCallbacks = [];

        /// <summary>
        /// Gets or sets the status code of the response.
        /// </summary>
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        /// <summary>
        /// Gets or sets the reason phrase of the response.
        /// </summary>
        public string? ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets the headers of the response.
        /// </summary>
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        /// <summary>
        /// Gets or sets the body of the response.
        /// </summary>
        public Stream Body { get; set; } = new MemoryStream();

        /// <summary>
        /// Gets a value indicating whether the response has started.
        /// </summary>
        public bool HasStarted { get; private set; }

        /// <summary>
        /// Registers a callback to invoke when the response starts.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">The state passed to the callback.</param>
        public void OnStarting(Func<object, Task> callback, object state)
        {
            _onStartingCallbacks.Add((callback, state));
        }

        /// <summary>
        /// Registers a callback to invoke when the response completes.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">The state passed to the callback.</param>
        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        /// <summary>
        /// Starts the response and invokes the registered callbacks in reverse registration order.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartAsync()
        {
            if (HasStarted)
                return;
            HasStarted = true;
            for (int index = _onStartingCallbacks.Count - 1; index >= 0; index--)
                await _onStartingCallbacks[index].Callback(_onStartingCallbacks[index].State);
        }
    }
}
