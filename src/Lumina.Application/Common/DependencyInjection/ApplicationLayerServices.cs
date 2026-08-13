#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
#endregion

namespace Lumina.Application.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Application layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ApplicationLayerServices
{
    /// <summary>
    /// Registers the services of the Application layer into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        // register the Mediator publisher used for publishing domain events
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        Type[] handlerContractTypes =
        [
            typeof(CQRS.ICommandHandler<,>),
            typeof(CQRS.IQueryHandler<,>),
            typeof(Infrastructure.Validation.IValidator<>),
        ];

        IEnumerable<Type> concreteTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => !type.IsInterface && !type.IsAbstract && !type.IsGenericTypeDefinition);

        var handlerRegistrations = concreteTypes
            .SelectMany(implementation => implementation.GetInterfaces(),
                (implementation, contract) => new { Contract = contract, Implementation = implementation })
            .Where(registration => registration.Contract.IsGenericType && handlerContractTypes.Contains(registration.Contract.GetGenericTypeDefinition()));

        foreach (var registration in handlerRegistrations)
        {
            if (services.Any(service => service.ServiceType == registration.Contract))
                continue;

            if (registration.Contract.GetGenericTypeDefinition() == typeof(Infrastructure.Validation.IValidator<>))
                services.AddSingleton(registration.Contract, registration.Implementation);
            else
                services.AddScoped(registration.Contract, registration.Implementation);
        }

        return services;
    }
}
