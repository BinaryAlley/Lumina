#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Infrastructure;
using Lumina.Infrastructure.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
#endregion

namespace Lumina.Infrastructure.Common.DependencyInjection;

/// <summary>
/// Contains all services of the Infrastructure layer.
/// </summary>
public static class InfrastructureLayerServices
{
    /// <summary>
    /// Extension method for adding the Infrastructure layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services)
    {
        // scan the current assembly for validators and add them to the DI container
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

        services.AddTransient<IHashService, HashService>();
        return services;
    }
}
