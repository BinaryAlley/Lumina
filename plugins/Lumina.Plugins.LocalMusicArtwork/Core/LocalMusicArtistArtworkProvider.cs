#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Core;

/// <summary>
/// Provides the artwork of an artist from the images stored in the folders of the local music library of the user.
/// </summary>
internal sealed class LocalMusicArtistArtworkProvider : IArtworkProvider<ArtistMetadataLookupDto>
{
    /// <summary>
    /// Gets the display name of the artwork provider.
    /// </summary>
    public string Name => "Local Music Artwork";

    /// <summary>
    /// Gets the media library types this artwork provider supports.
    /// </summary>
    public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.Music];

    /// <summary>
    /// Gets a value indicating whether this artwork provider requires access to the web to retrieve artwork.
    /// </summary>
    public bool RequiresWebAccess => false;

    /// <summary>
    /// Gets the artworks of the artist described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the artist to get the artwork for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artworks of the artist, or an empty collection when no artwork was found.</returns>
    public Task<IReadOnlyList<ArtworkDto>> GetArtworkAsync(ArtistMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return Task.FromResult(LocalMusicArtworkScanner.ScanArtistArtwork(lookup.Path));
    }
}
