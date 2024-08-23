#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Common.Configuration;
using Lumina.Presentation.Api.Common.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddPresentationApiLayerServices(this IServiceCollection services)
    {
        // add services to the container
        services.AddControllers();

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

        return services;
    }
    #endregion
}