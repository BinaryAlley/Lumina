#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using Lumina.Plugins.MusicBrainz.Core.Api;
using Lumina.Plugins.MusicBrainz.Core.Mapping;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.Core;

/// <summary>
/// Provides album metadata from MusicBrainz by resolving lookups into album metadata DTOs.
/// A MusicBrainz release group is the logical album, while one of its releases is selected as the edition the local files match.
/// </summary>
internal sealed class MusicBrainzAlbumMetadataProvider : IMetadataProvider<AlbumMetadataLookupDto, AlbumMetadataDto>
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
    /// Initializes a new instance of the <see cref="MusicBrainzAlbumMetadataProvider"/> class.
    /// </summary>
    /// <param name="musicBrainzHttpClient">The HTTP client used to call the MusicBrainz web service.</param>
    /// <param name="settingsProvider">The provider of the settings that configure the MusicBrainz web service requests.</param>
    public MusicBrainzAlbumMetadataProvider(MusicBrainzHttpClient musicBrainzHttpClient, MusicBrainzSettingsProvider settingsProvider)
    {
        _musicBrainzHttpClient = musicBrainzHttpClient;
        _settingsProvider = settingsProvider;
    }

    /// <summary>
    /// Searches for the metadata of the album described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the album to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of album metadata candidates found.</returns>
    public async Task<IReadOnlyList<AlbumMetadataDto>> GetSearchResultsAsync(AlbumMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(lookup.Title))
            return [];

        MusicBrainzSettingsDto settings = await _settingsProvider.GetAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<MusicBrainzReleaseGroupResponse> releaseGroups = await _musicBrainzHttpClient
            .SearchReleaseGroupsAsync(BuildReleaseGroupQuery(lookup.Title, lookup.ArtistName), settings.SearchResultLimit, cancellationToken).ConfigureAwait(false);

        List<AlbumMetadataDto> candidates = [];
        foreach (MusicBrainzReleaseGroupResponse releaseGroup in releaseGroups)
            if (!string.IsNullOrWhiteSpace(releaseGroup.Title))
                candidates.Add(MusicBrainzMapper.MapAlbum(releaseGroup, null));

        return candidates;
    }

    /// <summary>
    /// Gets the metadata of the album described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the album to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the album, or <see langword="null"/> when no album was found.</returns>
    public async Task<AlbumMetadataDto?> GetMetadataAsync(AlbumMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        MusicBrainzSettingsDto settings = await _settingsProvider.GetAsync(cancellationToken).ConfigureAwait(false);
        MusicBrainzReleaseResponse? release = null;
        MusicBrainzReleaseGroupResponse? releaseGroup = null;

        // A release identifier pins the exact edition, so it is fetched first, and its release group is then resolved for the album level fields.
        // The release is fetched together with its recordings, so that the track level enrichment of the same release is served from the response cache.
        if (lookup.MusicBrainzReleaseId is not null)
            release = await _musicBrainzHttpClient.GetReleaseWithRecordingsAsync(lookup.MusicBrainzReleaseId.Value, cancellationToken).ConfigureAwait(false);

        Guid? releaseGroupId = lookup.MusicBrainzReleaseGroupId;
        if (releaseGroupId is null && release?.ReleaseGroup?.Id is string releaseGroupIdValue && Guid.TryParse(releaseGroupIdValue, out Guid parsedReleaseGroupId))
            releaseGroupId = parsedReleaseGroupId;

        if (releaseGroupId is not null)
            releaseGroup = await _musicBrainzHttpClient.GetReleaseGroupAsync(releaseGroupId.Value, cancellationToken).ConfigureAwait(false);

        // Without any identifier, the album is searched by title and artist, and the first match is resolved in full.
        if (release is null && releaseGroup is null && !string.IsNullOrWhiteSpace(lookup.Title))
        {
            IReadOnlyList<MusicBrainzReleaseGroupResponse> results = await _musicBrainzHttpClient
                .SearchReleaseGroupsAsync(BuildReleaseGroupQuery(lookup.Title, lookup.ArtistName), 1, cancellationToken).ConfigureAwait(false);
            MusicBrainzReleaseGroupResponse? searchResult = results.Count > 0 ? results[0] : null;
            if (searchResult?.Id is string searchResultId && Guid.TryParse(searchResultId, out Guid parsedSearchResultId))
                releaseGroup = await _musicBrainzHttpClient.GetReleaseGroupAsync(parsedSearchResultId, cancellationToken).ConfigureAwait(false);
        }

        if (releaseGroup is null)
            return release is null ? null : MusicBrainzMapper.MapAlbum(new MusicBrainzReleaseGroupResponse { Id = release.ReleaseGroup?.Id, Title = release.Title }, release);

        // When the exact release was not pinned by the lookup, the best edition of the release group is selected.
        if (release is null && releaseGroup.Id is string releaseGroupIdentifier && Guid.TryParse(releaseGroupIdentifier, out Guid releaseGroupGuid))
        {
            IReadOnlyList<MusicBrainzReleaseResponse> releases = await _musicBrainzHttpClient
                .BrowseReleasesByReleaseGroupAsync(releaseGroupGuid, settings.ReleaseLookupLimit, cancellationToken).ConfigureAwait(false);
            release = SelectRelease(releases, lookup);
        }

        return MusicBrainzMapper.MapAlbum(releaseGroup, release);
    }

    /// <summary>
    /// Selects the release of a release group that best matches the lookup, favoring official releases and those released in the lookup year.
    /// </summary>
    /// <param name="releases">The releases to select from.</param>
    /// <param name="lookup">The lookup the selected release must match.</param>
    /// <returns>The best matching release, or <see langword="null"/> when no release was provided.</returns>
    private static MusicBrainzReleaseResponse? SelectRelease(IReadOnlyList<MusicBrainzReleaseResponse> releases, AlbumMetadataLookupDto lookup)
    {
        // Only a release whose tracklist matches the local album is selected, so that an edition of the release group that does not correspond to the
        // local files is never attached to the album; without a known local track count, no release can be confidently matched, so none is selected.
        if (lookup.TrackCount is not int trackCount)
            return null;

        List<MusicBrainzReleaseResponse> matchingReleases = [.. releases.Where(release => release.Media.Sum(medium => medium.TrackCount) == trackCount)];
        if (matchingReleases.Count == 0)
            return null;

        return matchingReleases
            .Select((release, index) => new
            {
                Release = release,
                Index = index,
                Score = ScoreRelease(release, lookup)
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Index)
            .Select(item => item.Release)
            .First();
    }

    /// <summary>
    /// Scores a release against the lookup to rank how well it matches.
    /// </summary>
    /// <param name="release">The release to score.</param>
    /// <param name="lookup">The lookup the release must match.</param>
    /// <returns>The match score of the release, where a higher score is a better match.</returns>
    private static int ScoreRelease(MusicBrainzReleaseResponse release, AlbumMetadataLookupDto lookup)
    {
        int score = 0;
        if (string.Equals(release.Status, "Official", StringComparison.OrdinalIgnoreCase))
            score += 10_000;

        if (lookup.ReleaseYear is int wantedYear && TryGetYear(release.Date) is int releaseYear && releaseYear == wantedYear)
            score += 1_000;

        if (!string.IsNullOrWhiteSpace(release.Barcode))
            score += 100;
        if (release.LabelInfo.Count > 0)
            score += 50;
        if (release.Media.Count > 0)
            score += 10;
        return score;
    }

    /// <summary>
    /// Gets the year of a MusicBrainz date string.
    /// </summary>
    /// <param name="date">The date string to read.</param>
    /// <returns>The year of the date, or <see langword="null"/> when it could not be read.</returns>
    private static int? TryGetYear(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return null;
        string year = date.Length >= 4 ? date[..4] : date;
        return int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : null;
    }

    /// <summary>
    /// Builds the MusicBrainz release group search query for the provided title and artist.
    /// </summary>
    /// <param name="title">The title of the release group.</param>
    /// <param name="artistName">The name of the artist, if applicable.</param>
    /// <returns>The built search query.</returns>
    private static string BuildReleaseGroupQuery(string title, string? artistName)
    {
        string query = $"releasegroup:\"{title.Trim().Replace("\"", string.Empty, StringComparison.Ordinal)}\"";
        if (!string.IsNullOrWhiteSpace(artistName))
            query += $" AND artist:\"{artistName.Trim().Replace("\"", string.Empty, StringComparison.Ordinal)}\"";
        return query;
    }
}
