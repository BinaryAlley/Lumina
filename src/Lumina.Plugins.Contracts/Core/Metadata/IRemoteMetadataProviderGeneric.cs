#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.Core.Metadata;

/// <summary>
/// Contract for a plugin that provides metadata for media items from a remote source, typed to the media item lookup and metadata types.
/// </summary>
/// <typeparam name="TLookup">The type of the lookup describing the media item.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata of the media item.</typeparam>
public interface IRemoteMetadataProvider<TLookup, TMetadata> : IRemoteMetadataProvider
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

    /// <inheritdoc/>
    async Task<IReadOnlyList<MetadataDto>> IRemoteMetadataProvider.GetSearchResultsAsync(MetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return await GetSearchResultsAsync((TLookup)lookup, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async Task<MetadataDto?> IRemoteMetadataProvider.GetMetadataAsync(MetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return await GetMetadataAsync((TLookup)lookup, cancellationToken).ConfigureAwait(false);
    }
}
