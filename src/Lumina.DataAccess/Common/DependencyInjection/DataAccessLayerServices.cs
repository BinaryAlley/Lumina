#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using Lumina.Application.Common.DataAccess.UoW;
#endregion

namespace Lumina.DataAccess.Common.DependencyInjection;

/// <summary>
/// Contains all services of the DataAccess layer.
/// </summary>
public static class DataAccessLayerServices
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Extension method for adding the DataAccess layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static void AddDataAccessLayerServices(this IServiceCollection services)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"The base path '{basePath}' does not exist.");
        services.AddDbContext<LuminaDbContext>(options =>
        {
            options.UseSqlite($"Data Source={Path.Combine(basePath, "Lumina.db")}");
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        Type[]? dataAccessLayerTypes = Assembly.GetExecutingAssembly().GetTypes();
        Type? genericRepositoryType = typeof(IRepository<>);
        Type? repositoryFactoryType = typeof(RepositoryFactory);
        Type? iRepositoryFactoryType = typeof(IRepositoryFactory);

        services.AddScoped(iRepositoryFactoryType!, repositoryFactoryType!);

        // get all classes implementing IRepository (all repository classes) and register them as their corresponding repository interface
        IEnumerable<Type> repositoryTypes = dataAccessLayerTypes.Where(t => !t.IsInterface &&
                                                                             t.GetInterfaces()
                                                                              .Any(i => i.IsGenericType &&
                                                                                        i.GetGenericTypeDefinition() == genericRepositoryType));
        foreach (Type type in repositoryTypes)
        {
            services.AddScoped(type.GetInterfaces() // TODO: change to scoped when DbContext is implemented
                                   .Where(i => !i.IsGenericType &&
                                                i.GetInterfaces()
                                                 .Any(j => j.GetGenericTypeDefinition() == genericRepositoryType))
                                   .First(), type);
        }
    }
    #endregion
}