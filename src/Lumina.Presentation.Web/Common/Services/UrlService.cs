#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Service for generating URLs from action and controller names.
/// </summary>
public class UrlService : IUrlService
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlService"/> class.
    /// </summary>
    /// <param name="linkGenerator">The ASP.NET Core link generator service.</param>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    public UrlService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Generates an absolute URL for the specified controller action.
    /// </summary>
    /// <param name="action">The action name within the controller.</param>
    /// <param name="controller">The controller name.</param>
    /// <returns>An absolute URL to the specified action.</returns>
    public string? GetAbsoluteUrl(string action, string controller)
    {
        string? scheme = _httpContextAccessor.HttpContext?.Request.Scheme;
        return _linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext!, action: action, controller: controller, values: null, scheme: scheme);
    }
}
