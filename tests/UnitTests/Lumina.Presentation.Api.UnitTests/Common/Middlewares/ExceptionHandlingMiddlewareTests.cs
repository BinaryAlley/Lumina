#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Common.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Common.Middlewares;

/// <summary>
/// Contains unit tests for the <see cref="ExceptionHandlingMiddleware"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ExceptionHandlingMiddlewareTests
{
    private readonly ILogger<ExceptionHandlingMiddleware> _mockLogger;
    private readonly ExceptionHandlingMiddleware _sut;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private RequestDelegate _nextDelegate = context => Task.CompletedTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddlewareTests"/> class.
    /// </summary>
    public ExceptionHandlingMiddlewareTests()
    {
        _mockLogger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        _sut = new ExceptionHandlingMiddleware(context => _nextDelegate(context), _mockLogger);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsException_ShouldWriteInternalServerErrorProblemDetails()
    {
        // Arrange
        DefaultHttpContext context = new()
        {
            Response = { Body = new MemoryStream() }
        };
        _nextDelegate = _ => throw new InvalidOperationException("sensitive internal details");

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(context.Response.Body, Encoding.UTF8);
        string responseBody = await reader.ReadToEndAsync();
        ProblemDetails problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, _jsonOptions)!;
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("InternalServerError", problemDetails.Title);
        Assert.Equal("An unexpected error occurred while processing the request.", problemDetails.Detail);
        Assert.False(string.IsNullOrEmpty(problemDetails.Extensions["traceId"]?.ToString()));
        Assert.DoesNotContain("sensitive internal details", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsException_ShouldLogTheException()
    {
        // Arrange
        DefaultHttpContext context = new()
        {
            Response = { Body = new MemoryStream() }
        };
        _nextDelegate = _ => throw new InvalidOperationException("sensitive internal details");

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Is<Exception>(exception => exception is InvalidOperationException && exception.Message == "sensitive internal details"),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsOperationCanceledException_ShouldRethrow()
    {
        // Arrange
        DefaultHttpContext context = new()
        {
            Response = { Body = new MemoryStream() }
        };
        _nextDelegate = _ => throw new OperationCanceledException();

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => _sut.InvokeAsync(context));

        // Assert
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseHasStarted_ShouldRethrowTheException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Features.Set<IHttpResponseFeature>(new StartedHttpResponseFeature());
        context.Response.Body = new MemoryStream();
        _nextDelegate = _ => throw new InvalidOperationException("sensitive internal details");

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.InvokeAsync(context));

        // Assert
        Assert.Equal(0, context.Response.Body.Length);
        _mockLogger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_WhenNextCompletesSuccessfully_ShouldNotModifyTheResponse()
    {
        // Arrange
        DefaultHttpContext context = new()
        {
            Response = { Body = new MemoryStream() }
        };
        _nextDelegate = context => Task.CompletedTask;

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    /// <summary>
    /// Response feature that reports the response as already started, for simulating a response that cannot be replaced.
    /// </summary>
    private sealed class StartedHttpResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string? ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = Stream.Null;

        public bool HasStarted => true;

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }
    }
}
