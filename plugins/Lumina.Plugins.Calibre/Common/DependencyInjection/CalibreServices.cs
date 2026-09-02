#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Calibre.Core;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Plugins.Calibre.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the Calibre metadata and artwork providers into the Dependency Injection container.
/// </summary>
internal static class CalibreServices
{
    /// <summary>
    /// Registers the services of the Calibre metadata provider into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the metadata.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection AddCalibreBookMetadataProvider(this IServiceCollection services, Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register the metadata provider as a keyed by pluginId transient service, so it can be resolved specifically among other IMetadataProvider implementations.
        services.AddKeyedTransient<IMetadataProvider, CalibreBookMetadataProvider>(pluginId);

        return services;
    }

    /// <summary>
    /// Registers the services of the Calibre artwork provider into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to register the services into.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the artwork.</param>
    /// <returns>The service collection, for chaining.</returns>
    internal static IServiceCollection AddCalibreArtworkProvider(this IServiceCollection services, Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register the artwork provider as a keyed by pluginId transient service, so it can be resolved specifically among other IArtworkProvider implementations.
        services.AddKeyedTransient<IArtworkProvider, CalibreArtworkProvider>(pluginId);

        return services;
    }
}
