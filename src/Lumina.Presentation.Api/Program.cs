#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using FastEndpoints.Swagger;
using Lumina.Application.Common.DependencyInjection;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.DependencyInjection;
using Lumina.Infrastructure.Common.DependencyInjection;
using Lumina.Presentation.Api.Common.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api;

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

        builder.Services.BindSharedConfiguration(builder.Configuration);
        builder.Services.BindPresentationApiLayerConfiguration(builder.Configuration);

        builder.Services.AddPresentationApiLayerServices(builder.Configuration);
        builder.Services.AddApplicationLayerServices();
        builder.Services.AddInfrastructureLayerServices();
        builder.Services.AddDataAccessLayerServices();
        builder.Services.AddDomainLayerServices();

        // determine log path based on environment
        string logPath;
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            logPath = Environment.GetEnvironmentVariable("LOG_PATH") ?? "/logs"; // use docker volume path
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5214); // HTTP only
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

        app.UseSerilogRequestLogging();

        // apply any pending migrations
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            try
            {
                LuminaDbContext context = services.GetRequiredService<LuminaDbContext>();
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                ILogger logger = services.GetRequiredService<ILogger>();
                logger.Fatal(ex, "Failed to run database migrations");
            }
        }

        app.UseCors("AllowAll");

        //app.UseExceptionHandler("/error"); // uses a middleware which re-executes the request to the  error path

        //app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseRateLimiter();

        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = "api";
            config.Versioning.Prefix = "v";
            config.Versioning.DefaultVersion = 1;
            config.Versioning.PrependToRoute = true;
            config.Endpoints.ShortNames = true;
        });
        app.UseSwaggerGen();

        string mediaRootDirectoryPathSetting = app.Configuration.GetValue<string>("MediaSettings:RootDirectory") ?? string.Empty;
        string mediaRootPath = Path.Combine(AppContext.BaseDirectory, mediaRootDirectoryPathSetting);

        if (!Directory.Exists(mediaRootPath))
            Directory.CreateDirectory(mediaRootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(mediaRootPath),
            RequestPath = "/media"
        });

        await app.RunAsync().ConfigureAwait(false);
    }
}
