#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Filters;
using Lumina.Presentation.Web.Common.MiddlewareFilters;
using Lumina.Presentation.Web.Common.Security;
using Lumina.Presentation.Web.Common.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        services.AddControllersWithViews(mvcOptions =>
        {
            mvcOptions.Filters.Add<ApiExceptionFilter>();
            mvcOptions.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipeline))); // add localization middleware filter
        })
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization()
        .AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.MaxDepth = 256;
            jsonOptions.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // needed because file system API responses can have very nested structures
            jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        // configure URL-based localization 
        services.AddLocalization(localizationOptions =>
        {
            localizationOptions.ResourcesPath = "Resources";
        });

        services.Configure<RequestLocalizationOptions>(requestLocalizationOptions =>
        {
            CultureInfo[] supportedCultures = 
            [
                new CultureInfo("de-DE"),
                new CultureInfo("en-US"),
                new CultureInfo("es-ES"),
                new CultureInfo("fr-FR"),
                new CultureInfo("it-IT"),
                new CultureInfo("ja-JP"),
                new CultureInfo("ro-RO"),
                new CultureInfo("ru-RU"),
                new CultureInfo("zh-CN"),
            ];
            requestLocalizationOptions.DefaultRequestCulture = new RequestCulture("en-US");
            requestLocalizationOptions.SupportedCultures = supportedCultures;
            requestLocalizationOptions.SupportedUICultures = supportedCultures;

            // configure route-based culture provider
            RouteDataRequestCultureProvider routeDataRequestCultureProvider = new()
            {
                RouteDataStringKey = "culture",
                UIRouteDataStringKey = "culture",
                Options = requestLocalizationOptions
            };
            // make the route culture provider the first one to be evaluated
            requestLocalizationOptions.RequestCultureProviders = 
            [
                routeDataRequestCultureProvider, // keep route-based culture handling
                new CookieRequestCultureProvider(), // but also add cookie option, so the application "remembers" the last used language the next time it's opened
                new AcceptLanguageHeaderRequestCultureProvider()
            ];
        });

        // configure cookie-based authentication
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(cookieAuthenticationOptions =>
            {
                // basic path configuration
                cookieAuthenticationOptions.LoginPath = "/auth/login";
                cookieAuthenticationOptions.LogoutPath = "/auth/logout";
                cookieAuthenticationOptions.AccessDeniedPath = "/auth/access-denied";

                // cookie configuration
                cookieAuthenticationOptions.Cookie = new CookieBuilder
                {
                    Name = ".Lumina.Auth", // unique name to avoid conflicts
                    HttpOnly = true,       // prevent JavaScript access
                    SameSite = SameSiteMode.Strict,
                    SecurePolicy = CookieSecurePolicy.Always, // require HTTPS
                    Path = "/",            // make cookie available for all paths
                    IsEssential = true     // mark as essential for GDPR
                };

                // security settings
                cookieAuthenticationOptions.ExpireTimeSpan = TimeSpan.FromHours(24); // TODO: perhaps make it configurable by user?
                cookieAuthenticationOptions.SlidingExpiration = true;

                // handle validation to support various deployment scenarios
                cookieAuthenticationOptions.Events = new CookieAuthenticationEvents
                {
                    // handle redirects to work with different base paths
                    OnRedirectToLogin = redirectContext =>
                    {
                        string culture = redirectContext.HttpContext.Request.RouteValues["culture"]?.ToString()?.ToLower() ?? "en-us";
                        // preserve any base path or subdirectory and add return URL
                        string redirectPath = redirectContext.RedirectUri;
                        // insert culture into the path
                        redirectPath = redirectPath.Replace("/auth/login", $"/{culture}/auth/login");
                        if (redirectContext.Request.PathBase.HasValue)
                            redirectPath = redirectContext.Request.PathBase + cookieAuthenticationOptions.LoginPath + new QueryString("?returnUrl=") + Uri.EscapeDataString(redirectContext.Request.PathBase + redirectContext.Request.Path);
                        redirectContext.Response.Redirect(redirectPath);
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogout = redirectContext =>
                    {
                        string culture = redirectContext.HttpContext.Request.RouteValues["culture"]?.ToString()?.ToLower() ?? "en-us";
                        string redirectPath = redirectContext.RedirectUri;
                        redirectPath = redirectPath.Replace("/auth/logout", $"/{culture}/auth/logout");
                        if (redirectContext.Request.PathBase.HasValue)
                            redirectPath = redirectContext.Request.PathBase + cookieAuthenticationOptions.LogoutPath;
                        redirectContext.Response.Redirect(redirectPath);
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = redirectContext =>
                    {
                        string culture = redirectContext.HttpContext.Request.RouteValues["culture"]?.ToString()?.ToLower() ?? "en-us";
                        string redirectPath = redirectContext.RedirectUri;
                        redirectPath = redirectPath.Replace("/auth/access-denied", $"/{culture}/auth/access-denied");
                        if (redirectContext.Request.PathBase.HasValue)
                            redirectPath = redirectContext.Request.PathBase + cookieAuthenticationOptions.AccessDeniedPath;
                        redirectContext.Response.Redirect(redirectPath);
                        return Task.CompletedTask;
                    }
                };
            });

        // add an authorization policy that ensures application is initialized with super admin account before allowing access
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireInitialization", authorizationPolicyBuilder => authorizationPolicyBuilder.Requirements.Add(new InitializationRequirement()));

        // add forwarded headers middleware to handle reverse proxy scenarios
        services.Configure<ForwardedHeadersOptions>(forwardedHeadersOptions =>
        {
            forwardedHeadersOptions.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
        });

        // configure session management to maintain user-specific data across requests
        services.AddSession(sessionOptions =>
        {
            sessionOptions.IdleTimeout = TimeSpan.FromMinutes(30); // session expires after 30 minutes of inactivity
            sessionOptions.Cookie.HttpOnly = true; // prevent JavaScript access to session cookie, for security
            sessionOptions.Cookie.IsEssential = true; // mark session cookie as essential, for GDPR compliance
        });

        // scan the current assembly for validators and register them to the DI container
        services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

        // handle transient errors like network timeouts or intermittent failures
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrInner<ApiException>(apiException =>
                apiException.HttpStatusCode != HttpStatusCode.BadRequest && // do not retry Bad Request and Forbidden responses, it's pointless
                apiException.HttpStatusCode != HttpStatusCode.Forbidden)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // use a circuit breaker to prevent repeatedly calling a failing service
        AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrInner<ApiException>(apiException => apiException.HttpStatusCode == HttpStatusCode.InternalServerError)
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
