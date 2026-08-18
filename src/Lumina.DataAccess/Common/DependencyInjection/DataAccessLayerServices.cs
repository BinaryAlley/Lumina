#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Seed;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.DataAccess.Common.Dapper;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Seed;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        // Type handlers are global static state, not DI services, and must be registered before any Dapper query executes;
        // the composition root runs at startup, before any query, so this is the correct place for the application.
        DapperTypeHandlers.Register();
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
        services.AddScoped<IDataSeedService, DataSeedService>();
    }
}
