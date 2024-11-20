#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Filters;

/// <summary>
/// Filters API exceptions and provides centralized handling for API-related errors.
/// </summary>
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly IStringLocalizer<ApiExceptionFilter> _stringLocalizer;
    private readonly ILogger<ApiExceptionFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiExceptionFilter"/> class with the specified localizer and logger.
    /// </summary>
    /// <param name="stringLocalizer">Injected service used for providing localized strings.</param>
    /// <param name="logger">Injected service used for logging.</param>
    public ApiExceptionFilter(IStringLocalizer<ApiExceptionFilter> stringLocalizer, ILogger<ApiExceptionFilter> logger)
    {
        _stringLocalizer = stringLocalizer;
        _logger = logger;
    }

    /// <summary>
    /// Called after an action has thrown an Exception.
    /// </summary>
    /// <param name="context">The <see cref="ExceptionContext"/>.</param>
    public void OnException(ExceptionContext context)
    {
        // check if the exception is an ApiException
        if (context.Exception is ApiException apiException)
        {
            // Log all available details for debugging/monitoring
            _logger.LogError(
                apiException,
                "API Exception occurred. Status: {StatusCode}, Title: {Title}, Detail: {Detail}, Extensions: {Extensions}",
                apiException.HttpStatusCode,
                apiException.ProblemDetails?.Title,
                apiException.ProblemDetails?.Detail,
                apiException.ProblemDetails?.Extensions);
            // handle unauthorized errors by prompting the user to re-login
            if (apiException.HttpStatusCode == HttpStatusCode.Unauthorized) // HTTP 401 Unauthorized actually means lack of valid authentication credentials (not logged in)
            {
                SignOutSynchronously(context.HttpContext);
                string currentUrl = context.HttpContext.Request.GetDisplayUrl();
                _logger.LogInformation("Redirecting unauthorized user to {Url}", currentUrl);
                context.HttpContext.Response.Redirect(currentUrl); // force an entire refresh of the current location, so that header and footer are re-rendered too
            }
            else
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
                    {
                        if (errorsObj.ValueKind == JsonValueKind.Object)
                        {
                            foreach (JsonProperty error in errorsObj.EnumerateObject())
                            {
                                _logger.LogError("Validation error {Field}: {@ErrorValues}", error.Name, error.Value.EnumerateArray().Select(jsonElement => jsonElement.ToString()));
                                if (error.Value.ValueKind == JsonValueKind.Array)
                                    foreach (JsonElement errorValue in error.Value.EnumerateArray())
                                        errorMessage.AppendLine($"{_stringLocalizer[errorValue.ToString()]}<br>");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogError(apiException, "Generic API error occurred");
                    errorMessage.AppendLine($"{apiException.Message}<br>");
                }
                // return a JSON result with the error details
                context.Result = new JsonResult(new
                {
                    success = false,
                    errorMessage = errorMessage.ToString()
                });
            }
        }
        else
        {
            _logger.LogError(context.Exception, "Unexpected exception occurred: {Message}", context.Exception.Message);
            // return a JSON result with the error details
            context.Result = new JsonResult(new
            {
                success = false,
                errorMessage = context.Exception.Message.ToString()
            });
        }
        // mark the exception as handled to prevent propagation
        context.ExceptionHandled = true;
    }

    /// <summary>
    /// Signs out the user synchronously by deleting the authentication cookie and token.
    /// </summary>
    /// <param name="httpContext">The HttpContext for the current request.</param>
    private static void SignOutSynchronously(HttpContext httpContext)
    {
        Task task = httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        task.Wait();  // run the task synchronously
        httpContext.Response.Cookies.Delete("Token");
    }
}
