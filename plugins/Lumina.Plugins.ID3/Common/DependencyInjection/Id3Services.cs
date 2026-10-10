#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.ID3.Core;
using Lumina.Plugins.ID3.Core.Tags;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
#endregion

namespace Lumina.Plugins.ID3.Common.DependencyInjection;

/// <summary>
/// Utility class for registering the services of the ID3 metadata provider into the Dependency Injection container.
/// </summary>
internal static class Id3Services
{
    /// <summary>
    /// Registers the services of the ID3 metadata providers into the Dependency Injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="pluginId">The unique identifier of the plugin that provides the metadata.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    internal static IServiceCollection AddId3MetadataProviders(this IServiceCollection services, Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(services);

        // The reader is scoped, so that the tags of a single file are read only once across the artist, album and track providers of the same enrichment scope.
        services.TryAddScoped<Id3TagReader>();

        // Register the metadata providers as keyed by pluginId transient services, so they can be resolved specifically among other IMetadataProvider implementations.
        // A single plugin exposes one provider per entity type, because the metadata contract is typed to a single lookup and metadata pair.
        services.AddKeyedTransient<IMetadataProvider, Id3ArtistMetadataProvider>(pluginId);
        services.AddKeyedTransient<IMetadataProvider, Id3AlbumMetadataProvider>(pluginId);
        services.AddKeyedTransient<IMetadataProvider, Id3TrackMetadataProvider>(pluginId);

        return services;
    }
}
