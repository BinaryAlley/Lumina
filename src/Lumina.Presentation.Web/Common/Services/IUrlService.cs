namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Interface for the service for generating URLs from action and controller names, with URL localization.
/// </summary>
public interface IUrlService
{
    /// <summary>
    /// Generates an absolute URL for the specified controller action.
    /// </summary>
    /// <param name="action">The action name within the controller.</param>
    /// <param name="controller">The controller name.</param>
    /// <returns>An absolute URL to the specified action.</returns>
    string? GetAbsoluteUrl(string action, string controller);
}
