#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.Core.Metadata;

/// <summary>
/// Contract for a plugin that provides artwork for media items, typed to the media item lookup type.
/// </summary>
/// <typeparam name="TLookup">The type of the lookup describing the media item.</typeparam>
public interface IArtworkProvider<TLookup> : IArtworkProvider
    where TLookup : MetadataLookupDto
{
    /// <summary>
    /// Gets the artworks of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to get the artwork for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artworks of the media item, or an empty collection when no artwork was found.</returns>
    Task<IReadOnlyList<ArtworkDto>> GetArtworkAsync(TLookup lookup, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the artworks of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to get the artwork for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artworks of the media item, or an empty collection when no artwork was found or the lookup is of another runtime type.</returns>
    async Task<IReadOnlyList<ArtworkDto>> IArtworkProvider.GetArtworkAsync(MetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return lookup is TLookup typedLookup ? await GetArtworkAsync(typedLookup, cancellationToken).ConfigureAwait(false) : [];
    }
}
