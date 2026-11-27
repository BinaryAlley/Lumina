#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using Lumina.Plugins.CoverArtArchive.Core.Api;
using Lumina.Plugins.CoverArtArchive.Core.Mapping;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Core;

/// <summary>
/// Provides the artwork of an album from the Cover Art Archive, by resolving the MusicBrainz identifiers of the album into artwork DTOs.
/// The artwork of the exact release is preferred, and the artwork of its release group is used when the release itself carries none.
/// </summary>
internal sealed class CoverArtArchiveAlbumArtworkProvider : IArtworkProvider<AlbumMetadataLookupDto>
{
    private readonly CoverArtArchiveHttpClient _coverArtArchiveHttpClient;

    /// <summary>
    /// Gets the display name of the artwork provider.
    /// </summary>
    public string Name => "Cover Art Archive";

    /// <summary>
    /// Gets the media library types this artwork provider supports.
    /// </summary>
    public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.Music];

    /// <summary>
    /// Gets a value indicating whether this artwork provider requires access to the web to retrieve artwork.
    /// </summary>
    public bool RequiresWebAccess => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoverArtArchiveAlbumArtworkProvider"/> class.
    /// </summary>
    /// <param name="coverArtArchiveHttpClient">The HTTP client used to call the Cover Art Archive API.</param>
    public CoverArtArchiveAlbumArtworkProvider(CoverArtArchiveHttpClient coverArtArchiveHttpClient)
    {
        _coverArtArchiveHttpClient = coverArtArchiveHttpClient;
    }

    /// <summary>
    /// Gets the artworks of the album described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the album to get the artwork for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artworks of the album, or an empty collection when no artwork was found.</returns>
    public async Task<IReadOnlyList<ArtworkDto>> GetArtworkAsync(AlbumMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        CoverArtArchiveArtworkResponse? artwork = null;

        // The identifier of the exact release pins the edition the local files match, so its artwork is preferred.
        if (lookup.MusicBrainzReleaseId is not null)
            artwork = await _coverArtArchiveHttpClient.GetReleaseArtworkAsync(lookup.MusicBrainzReleaseId.Value, cancellationToken).ConfigureAwait(false);

        // A release can carry no artwork of its own while its release group does, so the release group is the fallback.
        // The collection is guarded here too, because the deserializer can produce a null collection regardless of the initializer.
        if ((artwork?.Images is null || artwork.Images.Count == 0) && lookup.MusicBrainzReleaseGroupId is not null)
            artwork = await _coverArtArchiveHttpClient.GetReleaseGroupArtworkAsync(lookup.MusicBrainzReleaseGroupId.Value, cancellationToken).ConfigureAwait(false);

        return CoverArtArchiveMapper.MapArtwork(artwork);
    }
}
