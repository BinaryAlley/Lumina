namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Interface for the service for generating absolute URLs from the application route templates, with URL localization.
/// </summary>
public interface IUrlService
{
    /// <summary>
    /// Generates an absolute URL for the specified route template.
    /// </summary>
    /// <param name="routeTemplate">The route template of the target page or endpoint, taken from the <see cref="Lumina.Presentation.Web.Common.Routes.WebRoutes"/> constants.</param>
    /// <param name="additionalRouteValues">Optional additional route values, like route parameters.</param>
    /// <returns>An absolute URL to the specified route.</returns>
    string? GetAbsoluteUrl(string routeTemplate, object? additionalRouteValues = null);
}
