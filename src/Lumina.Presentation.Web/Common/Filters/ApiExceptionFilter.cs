#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Called after an action has thrown an Exception.
    /// </summary>
    /// <param name="context">The <see cref="ExceptionContext"/>.</param>
    public void OnException(ExceptionContext context)
    {
        // check if the exception is an ApiException
        if (context.Exception is ApiException apiException)
        {
            // handle unauthorized errors by prompting the user to re-login
            if (apiException.HttpStatusCode == HttpStatusCode.Unauthorized)
            {
                SignOutSynchronously(context.HttpContext);
                string currentUrl = context.HttpContext.Request.GetDisplayUrl();
                context.HttpContext.Response.Redirect(currentUrl); // force an entire refresh of the current location, so that header and footer are re-rendered too
            }
            else
            {
                StringBuilder errorMessage = new();
                if (apiException.ProblemDetails is not null)
                {
                    errorMessage.AppendLine($"HTTP {apiException.ProblemDetails.Status} {apiException.HttpStatusCode}<br>");
                    if (!string.IsNullOrEmpty(apiException.ProblemDetails.Detail))
                        errorMessage.AppendLine($"<b>{apiException.ProblemDetails.Detail}<b>");
                    else if (!string.IsNullOrEmpty(apiException.ProblemDetails.Title))
                        errorMessage.AppendLine($"<b>{apiException.ProblemDetails.Title}<b>");
                    if (apiException.ProblemDetails.Extensions is not null && apiException.ProblemDetails.Extensions.TryGetValue("errors", out JsonElement errorsObj))
                    {
                        if (errorsObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            errorMessage.AppendLine("Validation Errors:");
                            foreach (JsonProperty error in jsonElement.EnumerateObject())
                            {
                                string fieldName = error.Name;
                                if (error.Value.ValueKind == JsonValueKind.Array)
                                    foreach (JsonElement errorValue in error.Value.EnumerateArray())
                                        errorMessage.AppendLine($"- {fieldName}: {errorValue}<br>");
                            }
                        }
                    }
                    if (apiException.ProblemDetails.Extensions is not null)
                        foreach (KeyValuePair<string, JsonElement> extension in apiException.ProblemDetails.Extensions)
                            if (extension.Key != "errors")
                                errorMessage.AppendLine($"{extension.Key}: {extension.Value}<br>");
                }
                else
                {
                    errorMessage.AppendLine($"HTTP {apiException.HttpStatusCode}<br>");
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
