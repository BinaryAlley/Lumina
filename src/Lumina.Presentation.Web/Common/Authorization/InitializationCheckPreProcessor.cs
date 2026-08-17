#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// FastEndpoints pre-processor that runs the application initialization check before the endpoint handler executes.
/// </summary>
/// <remarks>
/// The check runs the <c>RequireInitialization</c> authorization policy, which queries the remote API and records the pending super admin setup state in the session.
/// Its result is deliberately ignored, because the pages that use this processor must be reachable both before and after the application is initialized.
/// </remarks>
/// <typeparam name="TRequest">The type of the request of the endpoint to which this pre-processor is applied.</typeparam>
public class InitializationCheckPreProcessor<TRequest> : IPreProcessor<TRequest> where TRequest : notnull
{
    /// <summary>
    /// Runs the initialization check for the current request.
    /// </summary>
    /// <param name="preProcessorContext">The pre-processor context for the current request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task PreProcessAsync(IPreProcessorContext<TRequest> preProcessorContext, CancellationToken cancellationToken)
    {
        HttpContext httpContext = preProcessorContext.HttpContext;
        Microsoft.AspNetCore.Authorization.IAuthorizationService authorizationService = httpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
        await authorizationService.AuthorizeAsync(httpContext.User, httpContext, AuthorizationPolicies.REQUIRE_INITIALIZATION).ConfigureAwait(false);
    }
}
