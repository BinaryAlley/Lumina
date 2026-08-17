#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Routes;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Reflection;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Service for generating absolute URLs for the application routes, with URL localization.
/// </summary>
public class UrlService : IUrlService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// The collection of route templates defined in the <see cref="WebRoutes"/> constants, used to validate the requested routes.
    /// </summary>
    private static readonly HashSet<string> s_knownRouteTemplates = GetKnownRouteTemplates();

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    public UrlService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Generates an absolute URL for the specified route template.
    /// </summary>
    /// <param name="routeTemplate">The route template of the target page or endpoint, taken from the <see cref="WebRoutes"/> constants.</param>
    /// <param name="additionalRouteValues">Optional additional route values, like route parameters.</param>
    /// <returns>An absolute URL to the specified route.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="routeTemplate"/> is not one of the route templates defined in the <see cref="WebRoutes"/> constants.</exception>
    public string? GetAbsoluteUrl(string routeTemplate, object? additionalRouteValues = null)
    {
        // fail fast when the route template is not one of the ones defined in the WebRoutes constants, instead of silently generating a broken URL
        if (!s_knownRouteTemplates.Contains(routeTemplate))
            throw new ArgumentException($"The route template '{routeTemplate}' is not defined in the WebRoutes constants.", nameof(routeTemplate));

        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return null;
        string culture = httpContext.Request.RouteValues["culture"]?.ToString() ?? "en-US";
        string path = routeTemplate.Replace("{culture}", culture, StringComparison.OrdinalIgnoreCase);
        if (additionalRouteValues is not null)
            foreach (PropertyInfo property in additionalRouteValues.GetType().GetProperties())
                path = path.Replace($"{{{property.Name}}}", property.GetValue(additionalRouteValues)?.ToString(), StringComparison.OrdinalIgnoreCase);
        // ensure the path starts with a slash, so that it is relative to the server root when concatenated with the host and the path base
        if (!path.StartsWith('/'))
            path = "/" + path;
        // build the absolute URL from the request's scheme, host and path base
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}{path}";
    }

    /// <summary>
    /// Collects the values of all route template constants defined in the <see cref="WebRoutes"/> class.
    /// </summary>
    /// <returns>The set of the route templates defined in the <see cref="WebRoutes"/> constants.</returns>
    private static HashSet<string> GetKnownRouteTemplates()
    {
        HashSet<string> routeTemplates = [];
        foreach (Type nestedType in typeof(WebRoutes).GetNestedTypes())
            foreach (FieldInfo field in nestedType.GetFields(BindingFlags.Public | BindingFlags.Static))
                if (field.IsLiteral && field.FieldType == typeof(string))
                    routeTemplates.Add((string)field.GetValue(null)!);
        return routeTemplates;
    }
}
