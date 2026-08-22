#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.DependencyInjection;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.DependencyInjection;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Infrastructure.Common.DependencyInjection;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Progress;
using Lumina.Presentation.Api.Common.DependencyInjection;
using Lumina.Presentation.Api.Common.Middlewares;
using Lumina.Presentation.Api.Common.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api;

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

        builder.Services.BindSharedConfiguration(builder.Configuration);
        builder.Services.BindPresentationApiLayerConfiguration(builder.Configuration);

        builder.Services.AddPresentationApiLayerServices(builder.Configuration);
        builder.Services.AddPresentationApiTelemetryServices(builder.Configuration, builder.Environment);
        builder.Services.AddApplicationLayerServices();
        builder.Services.AddInfrastructureLayerServices(builder.Configuration);
        builder.Services.AddDataAccessLayerServices();
        builder.Services.AddDomainLayerServices();

        // determine log path based on environment
        string logPath;
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            logPath = Environment.GetEnvironmentVariable("LOG_PATH") ?? "/logs"; // use docker volume path
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5214); // HTTP only; also, the port is also the same port exposed in the Dockerfile
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

                // enable the Write-Ahead Logging journal mode and a busy timeout, so that the Entity Framework connection and the dedicated connections
                // used by the bulk data access operations of Dapper can access the database concurrently, without database locking errors
                SqliteConnection sqliteConnection = new(context.Database.GetDbConnection().ConnectionString);
                sqliteConnection.Open();
                using SqliteCommand sqliteCommand = sqliteConnection.CreateCommand();
                sqliteCommand.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;";
                sqliteCommand.ExecuteNonQuery();
                sqliteConnection.Close();
            }
            catch (Exception ex)
            {
                ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogCritical(ex, "Failed to run database migrations");
            }
        }

        app.UseRouting();
        app.UseCors("SecurePolicy");

        app.UseApiExceptionHandling(); // catches any unhandled exception and returns a ProblemDetails response

        //app.UseHttpsRedirection();

        app.UseAuthentication();
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

        // add API documentation (OpenApi/Scalar)
        app.MapOpenApi();
        app.UseOpenApi(openApiDocumentMiddlewareSettings => openApiDocumentMiddlewareSettings.Path = "/openapi/{documentName}.json"); // this is needed for API versioning to work correctly with Scalar
        app.MapScalarApiReference(scalarOptions =>
        {
            scalarOptions.WithTitle("Lumina API")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                //.WithFavicon() // TODO: enable when favicon is added
                .WithDotNetFlag(true);
        });

        // add the middleware that fires domain events and ensures eventual transactional consistency, but NOT for long-polling/WebSockets stuff like SignalR, which would keep the db locked,
        // and NOT for the health probes, which would otherwise begin a database transaction on every probe
        app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/scanProgressHub") && !ctx.Request.Path.StartsWithSegments("/health"), app => app.UseMiddleware<EventualConsistencyMiddleware>());

        // create a directory relative to the application's startup directory, and use it to store static files that are served at the /media route on the API
        string mediaRootDirectoryPathSetting = app.Configuration.GetValue<string>("MediaSettings:RootDirectory") ?? string.Empty;
        string mediaRootPath = Path.Combine(AppContext.BaseDirectory, mediaRootDirectoryPathSetting);

        if (!Directory.Exists(mediaRootPath))
            Directory.CreateDirectory(mediaRootPath);

        // ensure the default file system structure is present
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;            
            IFileSystemStructureSeedService context = services.GetRequiredService<IFileSystemStructureSeedService>();
            Result<Created> setDefaultDirectoriesResult = context.SetDefaultDirectories(mediaRootPath);
            if (setDefaultDirectoriesResult.IsFailure)
                throw new InvalidOperationException("Could not create default file system directories structure: " + setDefaultDirectoriesResult.FirstError.Description);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(mediaRootPath),
            RequestPath = "/media"
        });

        app.MapHub<MediaLibraryScanProgressHub>("/scanProgressHub");

        // the liveness probe reports whether the process is running, while the readiness probe also verifies that the database is reachable
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthCheckResponseAsync
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponseAsync
        });

        await app.RunAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Writes the health report as a JSON response.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="healthReport">The health report to write.</param>
    private static async Task WriteHealthCheckResponseAsync(HttpContext httpContext, HealthReport healthReport)
    {
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new
        {
            status = healthReport.Status.ToString(),
            checks = healthReport.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            })
        });
    }
}
