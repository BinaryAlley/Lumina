#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
#endregion

namespace Lumina.Presentation.Web.Common.Filters.ActionFilters;

/// <summary>
/// Stores the current view path in session, for actions that return ViewResult. This is needed because when JWT expires, user is redirected to Login view, with a returnUrl.
/// This returnUrl needs to be that of the last displayed view, not of the call that triggered the re-authentication request, which can be an API call for data.
/// </summary>
public class StoreLastViewFilter : IActionFilter
{
    /// <summary>
    /// Executes before the action method is called
    /// </summary>
    /// <param name="context">The action executing context</param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // no action needed before execution
    }

    /// <summary>
    /// Executes after the action method is called
    /// </summary>
    /// <param name="context">The action executed context containing the action result</param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ViewResult)
        {
            HttpContext httpContext = context.HttpContext;
            if (httpContext.Request.Path != "/en-us/auth/login")
                httpContext.Session.SetString(HttpContextItemKeys.LAST_DISPLAYED_VIEW, $"{httpContext.Request.PathBase}{httpContext.Request.Path}{httpContext.Request.QueryString}");
        }
    }
}
