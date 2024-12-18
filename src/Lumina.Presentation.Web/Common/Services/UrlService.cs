#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Reflection;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Service for generating URLs from action and controller names, with URL localization.
/// </summary>
public class UrlService : IUrlService
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlService"/> class.
    /// </summary>
    /// <param name="linkGenerator">The ASP.NET Core link generator service.</param>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider"/> used to retrieve information about the application's routes and actions.</param>
    public UrlService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    }

    /// <summary>
    /// Generates an absolute URL for the specified controller action.
    /// </summary>
    /// <param name="action">The action name within the controller.</param>
    /// <param name="controller">The controller name.</param>
    /// <param name="additionalRouteValues">Optional additional route values, like query parameters.</param>
    /// <returns>An absolute URL to the specified action.</returns>
    public string? GetAbsoluteUrl(string action, string controller, object? additionalRouteValues = null)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return null;
        string scheme = httpContext.Request.Scheme;
        string culture = httpContext.Request.RouteValues["culture"]?.ToString() ?? "en-US";
        // find the controller's route template
        ControllerActionDescriptor? controllerActionDescriptor = _actionDescriptorCollectionProvider
            .ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .FirstOrDefault(controllerActionDescription =>
                controllerActionDescription.ControllerName.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                controllerActionDescription.ActionName.Equals(action, StringComparison.OrdinalIgnoreCase));
        if (controllerActionDescriptor is null)
            return null;
        // check if the route template contains {culture}
        string? routeTemplate = controllerActionDescriptor.AttributeRouteInfo?.Template;
        bool needsCulture = routeTemplate?.Contains("{culture}") == true;
        // merge culture and additional values
        RouteValueDictionary routeValues = [];
        if (needsCulture)
            routeValues.Add("culture", culture);
        if (additionalRouteValues is not null)
            foreach (PropertyInfo prop in additionalRouteValues.GetType().GetProperties())
                routeValues.Add(prop.Name, prop.GetValue(additionalRouteValues));

        // create route values based on whether the controller is localized
        object values = needsCulture ? new { culture, action } : new { action };
        return _linkGenerator.GetUriByAction(httpContext: httpContext, action: action, controller: controller, values: routeValues, scheme: scheme); 
    }
}
