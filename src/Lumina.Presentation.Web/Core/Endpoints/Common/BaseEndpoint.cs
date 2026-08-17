#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Http;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Common;

/// <summary>
/// Base class for Web endpoints.
/// </summary>
/// <typeparam name="TRequest">The type of the request object.</typeparam>
/// <typeparam name="TResponse">The type of the response object.</typeparam>
public abstract class BaseEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse> where TRequest : notnull
                                                                                        where TResponse : notnull
{
    /// <summary>
    /// Gets the current culture taken from the <c>culture</c> route value.
    /// </summary>
    protected string Culture => HttpContext.Request.RouteValues["culture"]?.ToString() ?? "en-US";

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    /// <remarks>
    /// All exceptions are propagated to the application's exception handling middleware instead of being converted to error responses by the library.
    /// </remarks>
    public override void Configure()
    {
        DontCatchExceptions();
    }

    /// <summary>
    /// Renders a Razor view to the response.
    /// </summary>
    /// <param name="viewName">The name or path of the view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="viewData">Optional additional view data entries made available to the view via <c>ViewData</c>.</param>
    /// <returns>An <see cref="IResult"/> that renders the view.</returns>
    protected IResult View(string viewName, object? model = null, IReadOnlyDictionary<string, object?>? viewData = null)
    {
        // store the path of the displayed view in the session, so that the login redirect after the JWT token expires points to it; ignore the login endpoint,
        // because it is incorrect to be redirected to the login URL, with the login URL as the return URL
        if (HttpContext.Request.Path != "/en-us/auth/login")
            HttpContext.Session.SetString(HttpContextItemKeys.LAST_DISPLAYED_VIEW, $"{HttpContext.Request.PathBase}{HttpContext.Request.Path}{HttpContext.Request.QueryString}");
        return new RazorViewResult(viewName, model, viewData);
    }

    /// <summary>
    /// Creates a JSON response indicating a successful operation, optionally wrapping <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The payload of the successful response.</param>
    /// <returns>An <see cref="IResult"/> containing the success JSON payload.</returns>
    protected IResult JsonSuccess(object? data)
    {
        return Results.Json(new { success = true, data });
    }

    /// <summary>
    /// Creates a JSON response indicating a successful operation without a payload.
    /// </summary>
    /// <returns>An <see cref="IResult"/> containing the success JSON payload.</returns>
    protected IResult JsonSuccess()
    {
        return Results.Json(new { success = true });
    }
}
