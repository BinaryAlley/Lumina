#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
#endregion

namespace Lumina.DataAccess.Common.DependencyInjection;

/// <summary>
/// Contains all services of the DataAccess layer.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DataAccessLayerServices
{
    /// <summary>
    /// Extension method for adding the DataAccess layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static void AddDataAccessLayerServices(this IServiceCollection services)
    {
        string? basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"The base path '{basePath}' does not exist.");
        services.AddDbContext<LuminaDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite($"Data Source={Path.Combine(basePath, "Lumina.db")}");
            options.AddInterceptors(serviceProvider.GetRequiredService<UpdateAuditableEntitiesInterceptor>());
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();

        Type[]? dataAccessLayerTypes = Assembly.GetExecutingAssembly().GetTypes();
        Type? genericRepositoryType = typeof(IRepository<>);
        Type? repositoryFactoryType = typeof(RepositoryFactory);
        Type? iRepositoryFactoryType = typeof(IRepositoryFactory);

        services.AddScoped(iRepositoryFactoryType!, repositoryFactoryType!);

        // get all classes implementing IRepository (all repository classes) and register them as their corresponding repository interface
        IEnumerable<Type> repositoryTypes = dataAccessLayerTypes.Where(type => !type.IsInterface &&
                                                                                type.GetInterfaces()
                                                                                    .Any(type => type.IsGenericType &&
                                                                                                 type.GetGenericTypeDefinition() == genericRepositoryType));
        foreach (Type type in repositoryTypes)
        {
            services.AddScoped(type.GetInterfaces() // TODO: change to scoped when DbContext is implemented
                                   .Where(type => !type.IsGenericType &&
                                                   type.GetInterfaces()
                                                       .Any(type => type.GetGenericTypeDefinition() == genericRepositoryType))
                                   .First(), type);
        }
    }
}
