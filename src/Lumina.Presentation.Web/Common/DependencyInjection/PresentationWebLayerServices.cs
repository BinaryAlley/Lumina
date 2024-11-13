#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Filters;
using Lumina.Presentation.Web.Common.Security;
using Lumina.Presentation.Web.Common.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Presentation Web layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class PresentationWebLayerServices
{
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

        // configure cookie-based authentication
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                // basic path configuration
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.AccessDeniedPath = "/auth/access-denied";

                // Cookie configuration
                options.Cookie = new CookieBuilder
                {
                    Name = ".Lumina.Auth", // unique name to avoid conflicts
                    HttpOnly = true,       // prevent JavaScript access
                    SameSite = SameSiteMode.Lax, // balance between security and functionality
                    SecurePolicy = CookieSecurePolicy.Always, // require HTTPS
                    Path = "/",            // make cookie available for all paths
                    IsEssential = true     // mark as essential for GDPR
                };

                // security settings
                options.ExpireTimeSpan = TimeSpan.FromHours(24); // TODO: perhaps make it configurable by user?
                options.SlidingExpiration = true;

                // handle validation to support various deployment scenarios
                options.Events = new CookieAuthenticationEvents
                {
                    // handle redirects to work with different base paths
                    OnRedirectToLogin = context =>
                    {
                        // preserve any base path or subdirectory
                        string redirectPath = context.RedirectUri;
                        if (context.Request.PathBase.HasValue)
                            redirectPath = context.Request.PathBase + options.LoginPath + new QueryString("?returnUrl=") + Uri.EscapeDataString(context.Request.PathBase + context.Request.Path);
                        context.Response.Redirect(redirectPath);
                        return Task.CompletedTask;
                    }
                };
            });

        // add an authorization policy that ensures application is initialized with super admin account before allowing access
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireInitialization", policy => policy.Requirements.Add(new InitializationRequirement()));

        // add forwarded headers middleware to handle reverse proxy scenarios
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // configure session management to maintain user-specific data across requests
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // session expires after 30 minutes of inactivity
            options.Cookie.HttpOnly = true; // prevent JavaScript access to session cookie, for security
            options.Cookie.IsEssential = true; // mark session cookie as essential, for GDPR compliance
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

        // register the HTTP typed client used for the API interaction
        services.AddHttpClient<IApiHttpClient, ApiHttpClient>()
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);

        // enable access to the current HTTP context in non-controller classes
        services.AddHttpContextAccessor();

        services.AddScoped<ApiExceptionFilter>();
        services.AddScoped<IAuthorizationHandler, InitializationHandler>();
        services.AddSingleton<ICryptographyService, CryptographyService>();
        services.AddSingleton<IUrlService, UrlService>();

        return services;
    }
}
