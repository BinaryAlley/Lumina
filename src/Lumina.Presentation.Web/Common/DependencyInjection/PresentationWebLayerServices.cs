#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Core.Services.UI;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Net;
using System.Net.Http;
#endregion

namespace Lumina.Presentation.Web.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Presentation Web layer.
/// </summary>
public static class PresentationWebLayerServices
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Registers the services of the Presentation Web layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPresentationWebLayerServices(this IServiceCollection services)
    {
        // scan the current assembly for validators and register them to the DI container
        services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);
        
        // handle transient errors like network timeouts or intermittent failures
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // use a circuit breaker to prevent repeatedly calling a failing service
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.InternalServerError)
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

        var policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

        // register the typed client used for the API interaction
        services.AddHttpClient<IApiHttpClient, ApiHttpClient>()
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy); 

        services.AddSingleton<ComboboxService>();

        return services;
    }
    #endregion
}