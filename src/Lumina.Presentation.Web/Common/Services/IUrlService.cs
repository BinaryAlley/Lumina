namespace Lumina.Presentation.Web.Common.Services;

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
