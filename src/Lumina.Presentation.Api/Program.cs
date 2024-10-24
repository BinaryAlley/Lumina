#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using FastEndpoints.Swagger;
using Lumina.Application.Common.DependencyInjection;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.Domain.Common.DependencyInjection;
using Lumina.Infrastructure.Common.DependencyInjection;
using Lumina.Presentation.Api.Common.DependencyInjection;
using Microsoft.AspNetCore.Builder;
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

        builder.Services.AddPresentationApiLayerServices();
        builder.Services.AddApplicationLayerServices();
        builder.Services.AddInfrastructureLayerServices();
        builder.Services.AddDataAccessLayerServices();
        builder.Services.AddDomainLayerServices();

        WebApplication app = builder.Build();

        app.UseCors("AllowAll");

        //app.UseExceptionHandler("/error"); // uses a middleware which re-executes the request to the  error path

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = "api";
            config.Versioning.Prefix = "v";
            config.Versioning.DefaultVersion = 1;
            config.Versioning.PrependToRoute = true;
            config.Endpoints.ShortNames = true;
        });
        app.UseSwaggerGen();

        await app.RunAsync().ConfigureAwait(false);
    }
}
