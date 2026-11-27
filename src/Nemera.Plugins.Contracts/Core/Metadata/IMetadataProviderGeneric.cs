#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.Core.Metadata;

/// <summary>
/// Contract for a plugin that provides metadata for media items, typed to the media item lookup and metadata types.
/// </summary>
/// <typeparam name="TLookup">The type of the lookup describing the media item.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata of the media item.</typeparam>
public interface IMetadataProvider<TLookup, TMetadata> : IMetadataProvider
    where TLookup : MetadataLookupDto
    where TMetadata : MetadataDto
{
    /// <summary>
    /// Searches for the metadata of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of metadata candidates found for the media item.</returns>
    Task<IReadOnlyList<TMetadata>> GetSearchResultsAsync(TLookup lookup, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the metadata of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the media item, or <see langword="null"/> when no metadata was found.</returns>
    Task<TMetadata?> GetMetadataAsync(TLookup lookup, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the type of the metadata lookup this provider accepts.
    /// </summary>
    Type IMetadataProvider.LookupType => typeof(TLookup);

    /// <summary>
    /// Searches for the metadata of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of metadata candidates found for the media item, or an empty collection when the lookup is of another runtime type.</returns>
    async Task<IReadOnlyList<MetadataDto>> IMetadataProvider.GetSearchResultsAsync(MetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return lookup is TLookup typedLookup ? await GetSearchResultsAsync(typedLookup, cancellationToken).ConfigureAwait(false) : [];
    }

    /// <summary>
    /// Gets the metadata of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the media item, or <see langword="null"/> when no metadata was found or the lookup is of another runtime type.</returns>
    async Task<MetadataDto?> IMetadataProvider.GetMetadataAsync(MetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return lookup is TLookup typedLookup ? await GetMetadataAsync(typedLookup, cancellationToken).ConfigureAwait(false) : null;
    }
}
