#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using Lumina.Plugins.MusicBrainz.Core.Api;
using Lumina.Plugins.MusicBrainz.Core.Mapping;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.Core;

/// <summary>
/// Provides music artist metadata from MusicBrainz by resolving lookups into artist metadata DTOs.
/// </summary>
internal sealed class MusicBrainzArtistMetadataProvider : IMetadataProvider<ArtistMetadataLookupDto, ArtistMetadataDto>
{
    private readonly MusicBrainzHttpClient _musicBrainzHttpClient;
    private readonly MusicBrainzSettingsProvider _settingsProvider;

    /// <summary>
    /// Gets the display name of the metadata provider.
    /// </summary>
    public string Name => "MusicBrainz";

    /// <summary>
    /// Gets the media library types this metadata provider supports.
    /// </summary>
    public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.Music];

    /// <summary>
    /// Gets a value indicating whether this metadata provider requires access to the web to retrieve metadata.
    /// </summary>
    public bool RequiresWebAccess => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicBrainzArtistMetadataProvider"/> class.
    /// </summary>
    /// <param name="musicBrainzHttpClient">The HTTP client used to call the MusicBrainz web service.</param>
    /// <param name="settingsProvider">The provider of the settings that configure the MusicBrainz web service requests.</param>
    public MusicBrainzArtistMetadataProvider(MusicBrainzHttpClient musicBrainzHttpClient, MusicBrainzSettingsProvider settingsProvider)
    {
        _musicBrainzHttpClient = musicBrainzHttpClient;
        _settingsProvider = settingsProvider;
    }

    /// <summary>
    /// Searches for the metadata of the artist described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the artist to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of artist metadata candidates found.</returns>
    public async Task<IReadOnlyList<ArtistMetadataDto>> GetSearchResultsAsync(ArtistMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(lookup.Name))
            return [];

        MusicBrainzSettingsDto settings = await _settingsProvider.GetAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<MusicBrainzArtistResponse> artists = await _musicBrainzHttpClient
            .SearchArtistsAsync(lookup.Name.Trim(), settings.SearchResultLimit, cancellationToken).ConfigureAwait(false);

        List<ArtistMetadataDto> candidates = [];
        foreach (MusicBrainzArtistResponse artist in artists)
            if (!string.IsNullOrWhiteSpace(artist.Name))
                candidates.Add(MusicBrainzMapper.MapArtist(artist));
        return candidates;
    }

    /// <summary>
    /// Gets the metadata of the artist described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the artist to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the artist, or <see langword="null"/> when no artist was found.</returns>
    public async Task<ArtistMetadataDto?> GetMetadataAsync(ArtistMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        // A MusicBrainz identifier is the most precise lookup, so it is used first, if present.
        if (lookup.MusicBrainzArtistId is not null)
        {
            MusicBrainzArtistResponse? artist = await _musicBrainzHttpClient.GetArtistAsync(lookup.MusicBrainzArtistId.Value, cancellationToken).ConfigureAwait(false);
            return artist is null ? null : MusicBrainzMapper.MapArtist(artist);
        }

        if (string.IsNullOrWhiteSpace(lookup.Name))
            return null;

        // Without an identifier, the artist is searched by name, and the first match is resolved in full.
        IReadOnlyList<MusicBrainzArtistResponse> artists = await _musicBrainzHttpClient
            .SearchArtistsAsync(lookup.Name.Trim(), 1, cancellationToken).ConfigureAwait(false);
        MusicBrainzArtistResponse? searchResult = artists.Count > 0 ? artists[0] : null;
        if (searchResult?.Id is null)
            return null;

        MusicBrainzArtistResponse? detailedArtist = System.Guid.TryParse(searchResult.Id, out System.Guid artistId)
            ? await _musicBrainzHttpClient.GetArtistAsync(artistId, cancellationToken).ConfigureAwait(false)
            : null;
        return detailedArtist is null ? MusicBrainzMapper.MapArtist(searchResult) : MusicBrainzMapper.MapArtist(detailedArtist);
    }
}
