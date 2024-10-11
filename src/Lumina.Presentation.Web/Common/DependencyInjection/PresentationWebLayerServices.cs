#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Filters;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Services.UI;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
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
        services.AddControllersWithViews(options => options.Filters.Add(typeof(ApiExceptionFilter)))
            .AddJsonOptions(options => 
            {
                options.JsonSerializerOptions.MaxDepth = 256;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // needed because file system API responses can have very nested structures
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // scan the current assembly for validators and register them to the DI container
        services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

        // handle transient errors like network timeouts or intermittent failures
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrInner<ApiException>(ex =>
                ex.HttpStatusCode != HttpStatusCode.BadRequest &&
                ex.HttpStatusCode != HttpStatusCode.Forbidden)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // use a circuit breaker to prevent repeatedly calling a failing service
        AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrInner<ApiException>(ex => ex.HttpStatusCode == HttpStatusCode.InternalServerError)
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

        AsyncPolicyWrap<HttpResponseMessage> policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

        // register the typed client used for the API interaction
        services.AddHttpClient<IApiHttpClient, ApiHttpClient>()
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);

        services.AddSingleton<ComboboxService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ApiExceptionFilter>();

        return services;
    }
    #endregion
}
