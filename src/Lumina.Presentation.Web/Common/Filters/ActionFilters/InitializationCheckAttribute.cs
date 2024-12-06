#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Filters.ActionFilters;

/// <summary>
/// Attribute that enforces application initialization checks on controller actions.
/// </summary>
public class InitializationCheckAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Asynchronously executes the initialization check before the action executes.
    /// </summary>
    /// <param name="context">The context for the action being executed.</param>
    /// <param name="next">Delegate to execute the next action filter in the pipeline.</param>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        IAuthorizationService authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        await authorizationService.AuthorizeAsync(context.HttpContext.User, context.HttpContext, "RequireInitialization").ConfigureAwait(false);
        await next().ConfigureAwait(false);
    }
}
