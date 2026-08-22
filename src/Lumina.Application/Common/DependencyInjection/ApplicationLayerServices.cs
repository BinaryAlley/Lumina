#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Telemetry;
using Lumina.Domain.Common.Events;
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
        Type[] handlerContractTypes =
        [
            typeof(CQRS.ICommandHandler<,>),
            typeof(CQRS.IQueryHandler<,>),
            typeof(Infrastructure.Validation.IValidator<>),
            typeof(IDomainEventHandler<>),
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
            {
                services.AddSingleton(registration.Contract, registration.Implementation);
                continue;
            }

            // wrap query, command, and domain event handlers in their telemetry decorator, so that every invocation emits traces, metrics, and structured logs
            services.AddScoped(registration.Contract, serviceProvider =>
            {
                object handler = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, registration.Implementation);
                Type decoratorType = TelemetryDecoratorFactory.GetDecoratorType(registration.Contract);
                return ActivatorUtilities.CreateInstance(serviceProvider, decoratorType, handler);
            });
        }

        return services;
    }
}
