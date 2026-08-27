#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Hashing;
using System.Linq;
using System.Text;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Computes the fingerprint of the provider configuration of a media library, used to detect whether the configuration
/// changed since the last scan, and thus whether the books of the library need their metadata or artwork re-enriched.
/// Any change to the configuration, like adding, removing, reordering, enabling, or disabling a provider, changes the fingerprint.
/// </summary>
internal static class ProviderConfigurationFingerprint
{
    /// <summary>
    /// Computes the fingerprint of the metadata provider configuration of a media library.
    /// </summary>
    /// <param name="configurations">The metadata provider configurations of the media library.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the books is aggregated from multiple providers, when fields are missing.</param>
    /// <param name="canDownloadMetadataFromWeb">Whether the media library permits downloading data from the web.</param>
    /// <returns>The fingerprint of the metadata provider configuration.</returns>
    public static string ComputeMetadataFingerprint(IReadOnlyList<LibraryMetadataProviderConfigurationEntity> configurations, bool shouldAggregateMetadataWhenMissing, bool canDownloadMetadataFromWeb)
    {
        StringBuilder canonicalRepresentation = new();
        foreach (LibraryMetadataProviderConfigurationEntity configuration in configurations.OrderBy(configuration => configuration.Rank))
            canonicalRepresentation.Append($"{configuration.PluginId:D}|{configuration.IsEnabled}|{configuration.Rank};");
        canonicalRepresentation.Append($"aggregate:{shouldAggregateMetadataWhenMissing};web:{canDownloadMetadataFromWeb};");

        return ComputeFingerprint(canonicalRepresentation.ToString());
    }

    /// <summary>
    /// Computes the fingerprint of the artwork provider configuration of a media library.
    /// </summary>
    /// <param name="configurations">The artwork provider configurations of the media library.</param>
    /// <param name="canDownloadMetadataFromWeb">Whether the media library permits downloading data from the web.</param>
    /// <returns>The fingerprint of the artwork provider configuration.</returns>
    public static string ComputeArtworkFingerprint(IReadOnlyList<LibraryArtworkProviderConfigurationEntity> configurations, bool canDownloadMetadataFromWeb)
    {
        StringBuilder canonicalRepresentation = new();
        foreach (LibraryArtworkProviderConfigurationEntity configuration in configurations.OrderBy(configuration => configuration.Rank))
            canonicalRepresentation.Append($"{configuration.PluginId:D}|{configuration.IsEnabled}|{configuration.Rank};");
        canonicalRepresentation.Append($"web:{canDownloadMetadataFromWeb};");

        return ComputeFingerprint(canonicalRepresentation.ToString());
    }

    /// <summary>
    /// Computes the fingerprint of the provided canonical representation, by hashing it.
    /// </summary>
    /// <param name="canonicalRepresentation">The canonical representation of the configuration.</param>
    /// <returns>The fingerprint of the canonical representation.</returns>
    private static string ComputeFingerprint(string canonicalRepresentation)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(canonicalRepresentation);
        ulong hash = XxHash64.HashToUInt64(bytes);
        return hash.ToString("X16", CultureInfo.InvariantCulture);
    }
}
