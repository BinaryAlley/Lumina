#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Behaviors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MapsterMapper;
using Mapster;
#endregion

namespace Lumina.Application.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Application layer.
/// </summary>
public static class ApplicationLayerServices
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Registers the services of the Application layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        // register Mediator
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // register the validation behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // register fluent validators
        services.AddValidatorsFromAssembly(typeof(ApplicationLayerServices).Assembly);

        // register mapster configs in this assembly
        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(typeof(ApplicationLayerServices).Assembly);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
    #endregion
}