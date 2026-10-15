#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Middlewares;

/// <summary>
/// Middleware to handle exceptions thrown when the remote API returns a problem details.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IStringLocalizer<ExceptionHandlingMiddleware> _stringLocalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="logger">Injected service used for logging.</param>
    /// <param name="stringLocalizer">Injected service used for providing localized strings.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IStringLocalizer<ExceptionHandlingMiddleware> stringLocalizer)
    {
        _next = next;
        _logger = logger;
        _stringLocalizer = stringLocalizer;
    }

    /// <summary>
    /// Handles incoming requests and handles any exceptions thrown.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away while its request or response was being processed, so there is nobody left to answer.
            context.Abort();
        }
        catch (ApiException apiException) // threat API exceptions differently - they contain Problem Details properties
        {
            await HandleApiExceptionAsync(context, apiException);
        }
        catch (Exception ex) // other cases - 
        {
            _logger.LogError(ex, "Unexpected exception occurred: {Message}", ex.Message);
            // Once the response has started, its status and headers are already on the wire, so they can no longer be replaced: the connection
            // is aborted instead, so that the client sees a failed transfer rather than a success response it cannot trust.
            if (context.Response.HasStarted)
            {
                context.Abort();
                return;
            }
            // return a JSON result with the error details
            await WriteJsonResponseAsync(context, new
            {
                success = false,
                errorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// Handles exceptions specific to the API, including problem details.
    /// </summary>
    private async Task HandleApiExceptionAsync(HttpContext context, ApiException apiException)
    {
        // log all available details for debugging/monitoring
        _logger.LogError(
            apiException,
            "API Exception occurred. Status: {StatusCode}, Title: {Title}, Detail: {Detail}, Extensions: {Extensions}",
            apiException.HttpStatusCode,
            apiException.ProblemDetails?.Title,
            apiException.ProblemDetails?.Detail,
            apiException.ProblemDetails?.Extensions);

        // Once the response has started, its status and headers are already on the wire, so they can no longer be replaced: the connection
        // is aborted instead, so that the client sees a failed transfer rather than a redirect it could not follow or a body it would misread.
        if (context.Response.HasStarted)
        {
            context.Abort();
            return;
        }

        // handle unauthorized errors by prompting the user to re-login
        if (apiException.HttpStatusCode == HttpStatusCode.Unauthorized) // HTTP 401 Unauthorized actually means lack of valid authentication credentials (not logged in)
            await HandleUnauthorizedAsync(context);
        else if (apiException.HttpStatusCode == HttpStatusCode.NotFound) // 
            await HandleNotFoundAsync(context, apiException);
        else if (apiException.HttpStatusCode == HttpStatusCode.Forbidden)
            await HandleForbiddenAsync(context, apiException);
        else
            await HandleGenericApiErrorAsync(context, apiException);
    }

    /// <summary>
    /// Handles not found exceptions.
    /// </summary>
    private async Task HandleNotFoundAsync(HttpContext context, ApiException apiException)
    {
        // when the API reports the currently logged-in user no longer exists (404 from the current-user authorization check),
        // the stored session is stale: sign out and send the user to the login page instead of the not-found page
        if (context.User?.Identity?.IsAuthenticated == true && IsCurrentUserAuthorizationCheck(apiException))
        {
            await HandleUnauthorizedAsync(context).ConfigureAwait(false);
            return;
        }

        if (IsJsonRequest(context))
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteJsonResponseAsync(context, new
            {
                success = false,
                errorMessage = "NotFound"
            });
        }
        else if (IsNavigationRequest(context))
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            string culture = context.Request.RouteValues["culture"]?.ToString()?.ToLower() ?? "en-us";
            string notFoundUrl = $"/{culture}/not-found";
            _logger.LogInformation("Redirecting user to NotFound url");
            context.Response.Redirect(notFoundUrl); // force an entire refresh of the current location, so that header and footer are re-rendered too
        }
        else
            await WriteStatusOnlyResponseAsync(context, (int)HttpStatusCode.NotFound).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles forbidden exceptions.
    /// </summary>
    private async Task HandleForbiddenAsync(HttpContext context, ApiException apiException)
    {
        if (IsJsonRequest(context))
        {
            string errorMessage = BuildErrorMessage(apiException);
            await WriteJsonResponseAsync(context, new
            {
                success = false,
                errorMessage
            });
        }
        else if (IsNavigationRequest(context))
            await context.ForbidAsync();
        else
            await WriteStatusOnlyResponseAsync(context, (int)HttpStatusCode.Forbidden).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the API exceptions that are neither authentication, authorization, nor not found failures.
    /// </summary>
    private async Task HandleGenericApiErrorAsync(HttpContext context, ApiException apiException)
    {
        // JSON callers and page navigations get the localized error message; any other caller, like an audio request, is answered with
        // a bare status code, because a JSON body is not a representation such a caller can consume.
        if (IsJsonRequest(context) || IsNavigationRequest(context))
        {
            string errorMessage = BuildErrorMessage(apiException);
            await WriteJsonResponseAsync(context, new
            {
                success = false,
                errorMessage
            });
        }
        else
            await WriteStatusOnlyResponseAsync(context, (int)apiException.HttpStatusCode).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles unauthorized exceptions.
    /// </summary>
    private async Task HandleUnauthorizedAsync(HttpContext context)
    {
        // when API JWT tokens expire, the client app is still logged in, so, sign out the user and delete the authentication cookie and token
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        context.Response.Cookies.Delete("Token");

        if (IsJsonRequest(context))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await WriteJsonResponseAsync(context, new
            {
                success = false,
                errorMessage = "Unauthorized"
            });
        }
        else if (IsNavigationRequest(context))
        {
            string culture = context.Request.RouteValues["culture"]?.ToString()?.ToLower() ?? "en-us";
            string returnUrl = $"{context.Request.Path}{context.Request.QueryString}";
            string loginUrl = $"/{culture}/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
            _logger.LogInformation("Redirecting unauthorized user to {Url}", loginUrl);
            context.Response.Redirect(loginUrl); // force an entire refresh of the current location, so that header and footer are re-rendered too
        }
        else
            await WriteStatusOnlyResponseAsync(context, (int)HttpStatusCode.Unauthorized).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds a detailed error message from an <see cref="ApiException"/>.
    /// </summary>
    private string BuildErrorMessage(ApiException apiException)
    {
        StringBuilder errorMessage = new();

        if (apiException.ProblemDetails is not null)
        {
            // only show the meaningful error message to the user
            if (!string.IsNullOrEmpty(apiException.ProblemDetails.Detail))
                errorMessage.AppendLine($"<b>{_stringLocalizer[apiException.ProblemDetails.Detail]}</b><br>");
            else if (!string.IsNullOrEmpty(apiException.ProblemDetails.Title))
                errorMessage.AppendLine($"<b>{_stringLocalizer[apiException.ProblemDetails.Title]}</b><br>");

            if (apiException.ProblemDetails.Extensions?.TryGetValue("errors", out JsonElement errorsObj) == true)
                if (errorsObj.ValueKind == JsonValueKind.Object)
                    foreach (JsonProperty error in errorsObj.EnumerateObject())
                    {
                        _logger.LogError("Validation error {Field}: {@ErrorValues}", error.Name, error.Value.EnumerateArray().Select(jsonElement => jsonElement.ToString()));
                        if (error.Value.ValueKind == JsonValueKind.Array)
                            foreach (JsonElement errorValue in error.Value.EnumerateArray())
                                errorMessage.AppendLine($"{_stringLocalizer[errorValue.ToString()]}<br>");
                    }
        }
        else
        {
            _logger.LogError(apiException, "Generic API error occurred");
            errorMessage.AppendLine($"<b>{apiException.Message}</b><br>");
            errorMessage.AppendLine($"{_stringLocalizer[apiException.HttpStatusCode.ToString()]}");
        }
        return errorMessage.ToString();
    }

    /// <summary>
    /// Checks if the current request is a programmatic request that expects a JSON response.
    /// </summary>
    /// <remarks>
    /// Every request is classified into exactly one of three kinds: a JSON request, which this method detects, a browser navigation, which
    /// <see cref="IsNavigationRequest"/> detects, and a raw request, which is neither of the two and is answered with a bare status code.
    /// </remarks>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns><see langword="true"/> when the request expects JSON, <see langword="false"/> otherwise.</returns>
    private static bool IsJsonRequest(HttpContext context)
    {
        return context.Request.Headers.Accept.Any(accept => accept?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
            || context.Request.Headers.XRequestedWith == "XMLHttpRequest";
    }

    /// <summary>
    /// Checks if the current request is a browser navigation, which is the only kind of request that can follow a redirect.
    /// </summary>
    /// <remarks>
    /// A request that explicitly asks for something other than HTML, like an audio or a media request, is answered with a bare status
    /// code: treating it as a page would redirect it to an HTML login or not-found page, which its caller cannot consume. A request that
    /// states no preference at all keeps the historical page behavior, so that callers that send no <c>Accept</c> header are not silently
    /// changed, but a request that carries no <c>Accept</c> yet asks for a byte range is treated as a raw request, because a headerless
    /// range request comes from a media client or a probe, not from a browser.
    /// </remarks>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns><see langword="true"/> when the request is a navigation, <see langword="false"/> otherwise.</returns>
    private static bool IsNavigationRequest(HttpContext context)
    {
        return (context.Request.Headers.Accept.Count == 0 && context.Request.Headers.Range.Count == 0)
            || context.Request.Headers.Accept.Any(accept => accept?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
            || string.Equals(context.Request.Headers["Sec-Fetch-Mode"].ToString(), "navigate", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.Request.Headers["Sec-Fetch-Dest"].ToString(), "document", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Writes a JSON response to the client.
    /// </summary>
    private static async Task WriteJsonResponseAsync(HttpContext context, object response)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// Writes the response of a raw request, which is a status code and no body.
    /// </summary>
    /// <remarks>
    /// This is the third branch of the request classification, the one that answers a request that is neither a JSON request, which
    /// <see cref="IsJsonRequest"/> detects, nor a browser navigation, which <see cref="IsNavigationRequest"/> detects.
    /// </remarks>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <param name="statusCode">The status code to answer with.</param>
    private async Task WriteStatusOnlyResponseAsync(HttpContext context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentLength = 0;
        try
        {
            // Starting the response commits the status code and the headers, so that no outer middleware replaces this decision with a page,
            // and so that a caller that asked for content never receives HTML instead. A token that is never cancelled is used on purpose,
            // because a request whose token is already cancelled would otherwise throw here, inside the exception handler, and escape it.
            await context.Response.StartAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception startException)
        {
            // There is nobody left to answer, so the connection is aborted, and the failure is only recorded for diagnostics.
            _logger.LogDebug(startException, "Could not commit the status only response, the client has likely disconnected.");
            context.Abort();
        }
    }

    /// <summary>
    /// Determines whether the API exception originated from the authorization check of the currently logged in user.
    /// </summary>
    /// <param name="apiException">The API exception to inspect.</param>
    /// <returns><see langword="true"/> if the exception came from the current-user authorization endpoint, <see langword="false"/> otherwise.</returns>
    private static bool IsCurrentUserAuthorizationCheck(ApiException apiException)
    {
        return apiException.RequestPath?.Contains("/auth/get-authorization", StringComparison.OrdinalIgnoreCase) == true;
    }
}
