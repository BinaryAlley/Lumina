#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DependencyInjection;
using Lumina.Presentation.Web.Common.HealthChecks;
using Lumina.Presentation.Web.Common.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Scalar.AspNetCore;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web;

/// <summary>
/// Application entry point, contains the composition root module, wires up all dependencies of the application.
/// </summary>
[ExcludeFromCodeCoverage]
public class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Optional command line arguments.</param>
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.BindConfiguration(builder.Configuration);
        builder.Services.AddPresentationWebLayerServices();
        builder.Services.AddPresentationWebTelemetryServices(builder.Configuration, builder.Environment);
        builder.Services.AddHealthChecks()
            .AddCheck<ApiReachabilityHealthCheck>("api", HealthStatus.Unhealthy, ["ready"]);

        // determine log path based on environment
        string logPath;
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            logPath = Environment.GetEnvironmentVariable("LOG_PATH") ?? "/logs"; // use docker volume path
            builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(5012)); // HTTP only
        }
        else
            logPath = Path.Combine(AppContext.BaseDirectory, "logs"); // use local binary path
        Directory.CreateDirectory(logPath);
        if (!logPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            logPath = logPath += Path.DirectorySeparatorChar;
        // set environment variable for Serilog configuration
        Environment.SetEnvironmentVariable("LOG_PATH", logPath);

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration)
                         .ReadFrom.Services(services)
                         .Enrich.FromLogContext();
        });

        WebApplication app = builder.Build();

        app.UseNotFoundRedirect();
        app.UseRequestLocalization();
        app.UseSerilogRequestLogging();

        // configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment() && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            app.UseExceptionHandler("/home/error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = staticFileResponseContext =>
            {
                // disable static file caching during development
                if (app.Environment.IsDevelopment())
                {
                    staticFileResponseContext.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                    staticFileResponseContext.Context.Response.Headers.Append("Expires", "-1");
                }
            },
            // prevent static files from being processed by routing
            ServeUnknownFileTypes = true
        });
        app.UseCultureRedirect(); // if the user attempts to go to a localized route without providing a culture, redirect to default culture
        app.UseApiExceptionHandling(); // handle any problem details returned by the API
        app.UseRouting();
        app.UseRequestLocalization(); // set the culture from the localized routes
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        // validate the antiforgery token after authentication, because the token is bound to the authenticated user and
        // validating it before the user is known would reject every request made by a logged-in user
        app.UseAntiforgeryFE(additionalContentTypes: ["application/json"]);
        app.UseForwardedHeaders();

        // handle path base from reverse proxies
        app.Use((context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues pathBase))
                context.Request.PathBase = pathBase.ToString();
            return next();
        });

        app.UseFastEndpoints();

        // add API documentation (OpenApi/Scalar), so that the endpoints exposed by the web application are discoverable and their contracts are visible
        app.MapOpenApi();
        app.UseOpenApi(openApiDocumentMiddlewareSettings => openApiDocumentMiddlewareSettings.Path = "/openapi/{documentName}.json");
        app.MapScalarApiReference(scalarOptions =>
        {
            scalarOptions.WithTitle("Lumina Web")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithDotNetFlag(true);
        });

        // liveness and readiness probes, probed by container orchestrators and load balancers; the readiness probe also
        // verifies that the API is reachable, so the web application is only reported ready when it can actually serve requests
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = healthCheck => healthCheck.Tags.Contains("ready") });

        await app.RunAsync();
    }
}
