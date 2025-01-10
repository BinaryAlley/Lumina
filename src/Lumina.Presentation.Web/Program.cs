#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DependencyInjection;
using Lumina.Presentation.Web.Common.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
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

        // determine log path based on environment
        string logPath;
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            logPath = Environment.GetEnvironmentVariable("LOG_PATH") ?? "/logs"; // use docker volume path
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5012); // HTTP only
            });
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
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseForwardedHeaders();

        // handle path base from reverse proxies
        app.Use((context, next) =>
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues pathBase))
                context.Request.PathBase = pathBase.ToString();
            return next();
        });

        // use localized routes for those that need localization
        app.MapControllerRoute(
            name: "localized",
            pattern: "{culture}/{*catchall}",
            constraints: new { culture = new RegexRouteConstraint("^[a-z]{2}(?:-[A-Z]{2})?$") }
        );
        // for the rest, use default routing
        app.MapControllerRoute(
            name: "default",
            pattern: "{*catchall}"
        );

        await app.RunAsync();
    }
}
