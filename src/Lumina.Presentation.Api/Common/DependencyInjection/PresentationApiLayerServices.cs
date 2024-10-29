#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Presentation API layer.
/// </summary>
public static class PresentationApiLayerServices
{
    /// <summary>
    /// Registers the services of the Presentation API layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPresentationApiLayerServices(this IServiceCollection services)
    {

        services.Configure<JsonOptions>(jsonOptions =>
        {
            jsonOptions.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.SerializerOptions.MaxDepth = 256;
            jsonOptions.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // needed because file system API responses can have very nested structures
        });

        services.AddAuthorization();

        services.AddFastEndpoints(
        o => o.Assemblies =
        [
            typeof(Program).Assembly
        ]);
        services.SwaggerDocument(documentOptions =>
        {
            documentOptions.SerializerSettings = s =>
            {
                s.PropertyNamingPolicy = null;
                s.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            };
            documentOptions.MaxEndpointVersion = 1;
            documentOptions.MinEndpointVersion = 1;
            documentOptions.DocumentSettings = s =>
            {
                s.DocumentName = "Release 1.0";
                s.Title = "Lumina API";
                s.Version = "v1.0";
            };
            documentOptions.RemoveEmptyRequestSchema = true;
            documentOptions.ShortSchemaNames = true;            
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                 builder => builder
                     .AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader());
        });

        return services;
    }
}
