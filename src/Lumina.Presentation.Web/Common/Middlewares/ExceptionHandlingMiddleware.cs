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
        catch (ApiException apiException) // threat API exceptions differently - they contain Problem Details properties
        {
            await HandleApiExceptionAsync(context, apiException);
        }
        catch (Exception ex) // other cases - 
        {
            _logger.LogError(ex, "Unexpected exception occurred: {Message}", ex.Message);
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

        // handle unauthorized errors by prompting the user to re-login
        if (apiException.HttpStatusCode == HttpStatusCode.Unauthorized) // HTTP 401 Unauthorized actually means lack of valid authentication credentials (not logged in)
            await HandleUnauthorizedAsync(context);
        else if (apiException.HttpStatusCode == HttpStatusCode.NotFound) // 
            await HandleNotFoundAsync(context);
        else if (apiException.HttpStatusCode == HttpStatusCode.Forbidden)
        {
            if (IsApiRequest(context))
            {
                string errorMessage = BuildErrorMessage(apiException);
                await WriteJsonResponseAsync(context, new
                {
                    success = false,
                    errorMessage
                });
            }
            else
                await context.ForbidAsync();
        }
        else
        {
            string errorMessage = BuildErrorMessage(apiException);
            await WriteJsonResponseAsync(context, new
            {
                success = false,
                errorMessage
            });
        }
    }

    /// <summary>
    /// Handles not found exceptions.
    /// </summary>
    private async Task HandleNotFoundAsync(HttpContext context)
    {      
        if (IsApiRequest(context))
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteJsonResponseAsync(context, new
            {
                success = false,
                errorMessage = "NotFound"
            });
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            string culture = context.Request.RouteValues["culture"]?.ToString()?.ToLower() ?? "en-us";
            string notFoundUrl = $"/{culture}/not-found";
            _logger.LogInformation("Redirecting user to NotFound url");
            context.Response.Redirect(notFoundUrl); // force an entire refresh of the current location, so that header and footer are re-rendered too
        }
    }

    /// <summary>
    /// Handles unauthorized exceptions.
    /// </summary>
    private async Task HandleUnauthorizedAsync(HttpContext context)
    {
        // when API JWT tokens expire, the client app is still logged in, so, sign out the user and delete the authentication cookie and token
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        context.Response.Cookies.Delete("Token");

        if (IsApiRequest(context))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await WriteJsonResponseAsync(context, new
            {
                success = false,
                errorMessage = "Unauthorized"
            });
        }
        else
        {
            string culture = context.Request.RouteValues["culture"]?.ToString()?.ToLower() ?? "en-us";
            string returnUrl = $"{context.Request.Path}{context.Request.QueryString}";
            string loginUrl = $"/{culture}/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
            _logger.LogInformation("Redirecting unauthorized user to {Url}", loginUrl);
            context.Response.Redirect(loginUrl); // force an entire refresh of the current location, so that header and footer are re-rendered too
        }
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
    /// Checks if the current request is an API request.
    /// </summary>
    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Headers.Accept.Any(x => x?.Contains("application/json") == true) || context.Request.Headers.XRequestedWith == "XMLHttpRequest";
    }

    /// <summary>
    /// Writes a JSON response to the client.
    /// </summary>
    private static async Task WriteJsonResponseAsync(HttpContext context, object response)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
