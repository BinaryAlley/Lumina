#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Seed;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.DataAccess.Common.Dapper;
using Nemera.DataAccess.Common.Interceptors;
using Nemera.DataAccess.Core.Seed;
using Nemera.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
#endregion

namespace Nemera.DataAccess.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the Data Access layer into the Dependency Injection container.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DataAccessLayerServices
{
    /// <summary>
    /// Registers the services of the Data Access layer into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static void AddDataAccessLayerServices(this IServiceCollection services)
    {
        // Type handlers are global static state, not DI services, and must be registered before any Dapper query executes;
        // the composition root runs at startup, before any query, so this is the correct place for the application.
        DapperTypeHandlers.Register();
        string? basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"The base path '{basePath}' does not exist.");
        services.AddDbContext<NemeraDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite($"Data Source={Path.Combine(basePath, "Nemera.db")}");
            options.AddInterceptors(serviceProvider.GetRequiredService<UpdateAuditableEntitiesInterceptor>());
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();
        services.AddScoped<IDataSeedService, DataSeedService>();
    }
}
