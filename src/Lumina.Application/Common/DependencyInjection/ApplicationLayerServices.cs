#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Behaviors;
using Mediator;
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
        // register Mediator
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // register the validation behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // register fluent validators
        services.AddValidatorsFromAssembly(typeof(ApplicationLayerServices).Assembly);

        Type[] handlers =
        [
            typeof(CQRS.ICommandHandler<,>),
            typeof(CQRS.IQueryHandler<,>),
            typeof(Infrastructure.Validation.IValidator<>),
        ];

        IEnumerable<Type> types = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => !type.IsInterface && !type.IsAbstract);

        var registrations = types
                    .SelectMany(type => type.GetInterfaces(),
                        (implementation, service) => new { Service = service, Implmentation = implementation })
                    .Where(type => type.Service.IsGenericType && handlers.Contains(type.Service.GetGenericTypeDefinition()));

        foreach (var registration in registrations)
        {
            if (!services.Any(service => service.ServiceType == registration.Service))
            {
                if (registration.Service.GetGenericTypeDefinition() == typeof(Infrastructure.Validation.IValidator<>))
                    services.AddSingleton(registration.Service, registration.Implmentation);
                else
                    services.AddScoped(registration.Service, registration.Implmentation);
            }
        }
        return services;
    }
}
