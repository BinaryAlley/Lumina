#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Behaviors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
#endregion

namespace Lumina.Application.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Application layer.
/// </summary>
public static class ApplicationLayerServices
{
    /// <summary>
    /// Registers the services of the Application layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
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

        return services;
    }
}
