#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.LocalMusicArtwork.Common.Models.DTO.Settings;
using Lumina.Plugins.LocalMusicArtwork.Core.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Core;

/// <summary>
/// Provides the artwork of an album from the images stored in the folders of the local music library of the user.
/// When the album has no cover image on disk and the settings allow it, the cover embedded in the audio files is extracted as a fallback.
/// </summary>
internal sealed class LocalMusicAlbumArtworkProvider : IArtworkProvider<AlbumMetadataLookupDto>
{
    private readonly LocalMusicArtworkSettingsProvider _settingsProvider;

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
    /// Initializes a new instance of the <see cref="LocalMusicAlbumArtworkProvider"/> class.
    /// </summary>
    /// <param name="settingsProvider">The provider of the runtime settings of the plugin.</param>
    public LocalMusicAlbumArtworkProvider(LocalMusicArtworkSettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
    }

    /// <summary>
    /// Gets the artworks of the album described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the album to get the artwork for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artworks of the album, or an empty collection when no artwork was found.</returns>
    public async Task<IReadOnlyList<ArtworkDto>> GetArtworkAsync(AlbumMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        IReadOnlyList<ArtworkDto> artworks = LocalMusicArtworkScanner.ScanAlbumArtwork(lookup.Path);

        // The cover stored on disk always wins; the embedded one only fills the gap when the album carries no cover on disk at all.
        if (artworks.Any(artwork => artwork.Type == ArtworkType.Cover))
            return artworks;

        LocalMusicArtworkSettingsDto settings = await _settingsProvider.GetAsync(cancellationToken).ConfigureAwait(false);
        if (!settings.ShouldExtractEmbeddedCover)
            return artworks;

        ArtworkDto? embeddedCover = LocalMusicArtworkEmbeddedCoverExtractor.Extract(lookup.Path);
        if (embeddedCover is null)
            return artworks;
        return [.. artworks, embeddedCover];
    }
}
