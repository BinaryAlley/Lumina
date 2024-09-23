#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Converters;
using Lumina.Presentation.Api.Common.Configuration;
using Lumina.Presentation.Api.Common.Errors;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Presentation API layer.
/// </summary>
public static class PresentationApiLayerServices
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Registers the services of the Presentation API layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPresentationApiLayerServices(this IServiceCollection services)
    {
        services.AddCors(options => options.AddPolicy("AllowAll",
                 builder => builder
                     .AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader()));

        // add services to the container
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverterFactory());
        });

        // add services to the container.
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();

        // register mapster configs
        TypeAdapterConfig mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(typeof(PresentationApiLayerServices).Assembly);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
    #endregion
}