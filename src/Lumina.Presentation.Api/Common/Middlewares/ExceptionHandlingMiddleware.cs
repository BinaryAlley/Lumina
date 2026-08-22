#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Common.Middlewares;

/// <summary>
/// Middleware that catches unhandled exceptions and converts them into a ProblemDetails response.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="logger">Injected service used for logging.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Handles incoming requests and converts any unhandled exception into a ProblemDetails response.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // the request was aborted, so there is no client left to write a response to
            throw;
        }
        catch (Exception ex)
        {
            // once the response has started, it is no longer possible to replace it with a ProblemDetails response
            if (context.Response.HasStarted)
                throw;
            _logger.LogError(ex, "Unhandled exception occurred while processing request {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemDetailsResponseAsync(context).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Writes a generic ProblemDetails response, without exposing any internal exception details to the client.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    private static async Task WriteProblemDetailsResponseAsync(HttpContext context)
    {
        ProblemDetails problemDetails = new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "InternalServerError",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred while processing the request.",
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        JsonSerializerOptions? jsonSerializerOptions = null;
        await context.Response.WriteAsJsonAsync(problemDetails, jsonSerializerOptions, "application/problem+json", context.RequestAborted).ConfigureAwait(false);
    }
}
