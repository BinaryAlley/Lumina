#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web;

/// <summary>
/// Application entry point, contains the composition root module, wires up all dependencies of the application.
/// </summary>
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
            logPath = Environment.GetEnvironmentVariable("LOG_PATH") ?? "/logs"; // use docker volume path
        else
            logPath = Path.Combine(AppContext.BaseDirectory, "logs"); // use local binary path
        Directory.CreateDirectory(logPath);
        // set environment variable for Serilog configuration
        Environment.SetEnvironmentVariable("LOG_PATH", logPath);

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration)
                         .ReadFrom.Services(services)
                         .Enrich.FromLogContext();
        });

        WebApplication app = builder.Build();

        app.UseSerilogRequestLogging();

        // configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles();
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


        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        await app.RunAsync();
    }
}
