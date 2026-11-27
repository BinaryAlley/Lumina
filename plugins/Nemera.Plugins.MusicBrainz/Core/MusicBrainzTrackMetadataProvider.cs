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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.Core;

/// <summary>
/// Provides track metadata from MusicBrainz by resolving lookups into audio metadata DTOs.
/// </summary>
internal sealed class MusicBrainzTrackMetadataProvider : IMetadataProvider<TrackMetadataLookupDto, AudioMetadataDto>
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
    /// Initializes a new instance of the <see cref="MusicBrainzTrackMetadataProvider"/> class.
    /// </summary>
    /// <param name="musicBrainzHttpClient">The HTTP client used to call the MusicBrainz web service.</param>
    /// <param name="settingsProvider">The provider of the settings that configure the MusicBrainz web service requests.</param>
    public MusicBrainzTrackMetadataProvider(MusicBrainzHttpClient musicBrainzHttpClient, MusicBrainzSettingsProvider settingsProvider)
    {
        _musicBrainzHttpClient = musicBrainzHttpClient;
        _settingsProvider = settingsProvider;
    }

    /// <summary>
    /// Searches for the metadata of the track described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the track to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of track metadata candidates found.</returns>
    public async Task<IReadOnlyList<AudioMetadataDto>> GetSearchResultsAsync(TrackMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(lookup.Title))
            return [];

        MusicBrainzSettingsDto settings = await _settingsProvider.GetAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<MusicBrainzRecordingResponse> recordings = await _musicBrainzHttpClient
            .SearchRecordingsAsync(BuildRecordingQuery(lookup), settings.SearchResultLimit, cancellationToken).ConfigureAwait(false);

        List<AudioMetadataDto> candidates = [];
        foreach (MusicBrainzRecordingResponse recording in recordings)
            if (!string.IsNullOrWhiteSpace(recording.Title))
                candidates.Add(MusicBrainzMapper.MapTrack(recording, null, null));
        return candidates;
    }

    /// <summary>
    /// Gets the metadata of the track described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the track to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the track, or <see langword="null"/> when no track was found.</returns>
    public async Task<AudioMetadataDto?> GetMetadataAsync(TrackMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        // When the release the track belongs to is known, the release is fetched once, with all its recordings, and the track is resolved from its
        // tracklist; the recording of that response already carries its work, so every track of the release is enriched from a single request.
        if (lookup.MusicBrainzReleaseId is not null)
        {
            MusicBrainzReleaseResponse? release = await _musicBrainzHttpClient.GetReleaseWithRecordingsAsync(lookup.MusicBrainzReleaseId.Value, cancellationToken).ConfigureAwait(false);
            MusicBrainzTrackResponse? releaseTrack = release is null ? null : FindTrack(release, lookup);
            if (releaseTrack?.Recording is not null)
            {
                MusicBrainzWorkResponse? inlineWork = await ResolveFallbackWorkAsync(ResolveInlineWork(releaseTrack.Recording), lookup, cancellationToken).ConfigureAwait(false);
                return MusicBrainzMapper.MapTrack(releaseTrack.Recording, inlineWork, release);
            }
        }

        MusicBrainzRecordingResponse? recording = null;

        // A recording identifier is the most precise lookup, so it is used next, if present.
        if (lookup.MusicBrainzRecordingId is not null)
            recording = await _musicBrainzHttpClient.GetRecordingAsync(lookup.MusicBrainzRecordingId.Value, cancellationToken).ConfigureAwait(false);

        // An ISRC lookup returns the recordings that carry the code, the first of which is then resolved in full.
        if (recording is null && !string.IsNullOrWhiteSpace(lookup.Isrc))
        {
            IReadOnlyList<MusicBrainzRecordingResponse> recordings = await _musicBrainzHttpClient
                .GetRecordingsByIsrcAsync(lookup.Isrc.Trim(), cancellationToken).ConfigureAwait(false);
            MusicBrainzRecordingResponse? match = recordings.FirstOrDefault();
            recording = await ResolveRecordingAsync(match, cancellationToken).ConfigureAwait(false);
        }

        // Without an identifier or a code, the track is searched, and the first match is resolved in full.
        if (recording is null && !string.IsNullOrWhiteSpace(lookup.Title))
        {
            IReadOnlyList<MusicBrainzRecordingResponse> recordings = await _musicBrainzHttpClient
                .SearchRecordingsAsync(BuildRecordingQuery(lookup), 1, cancellationToken).ConfigureAwait(false);
            recording = await ResolveRecordingAsync(recordings.FirstOrDefault(), cancellationToken).ConfigureAwait(false);
        }

        if (recording is null)
            return null;

        MusicBrainzWorkResponse? work = await ResolveWorkAsync(recording, cancellationToken).ConfigureAwait(false);
        work = await ResolveFallbackWorkAsync(work, lookup, cancellationToken).ConfigureAwait(false);
        MusicBrainzReleaseResponse? resolvedRelease = await ResolveReleaseAsync(recording, cancellationToken).ConfigureAwait(false);
        return MusicBrainzMapper.MapTrack(recording, work, resolvedRelease);
    }

    /// <summary>
    /// Falls back to the work identifier of the tags when MusicBrainz did not link the recording to a work.
    /// </summary>
    /// <param name="work">The work resolved from MusicBrainz, if any.</param>
    /// <param name="lookup">The lookup describing the local track.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The provided work, the work resolved from the work identifier of the tags, or <see langword="null"/> when neither is available.</returns>
    private async Task<MusicBrainzWorkResponse?> ResolveFallbackWorkAsync(MusicBrainzWorkResponse? work, TrackMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        // The title of the tags means the work of the tags is already complete, so it is kept as is and MusicBrainz is not queried again for it.
        if (work is not null || lookup.MusicBrainzWorkId is not Guid musicBrainzWorkId || !string.IsNullOrWhiteSpace(lookup.WorkTitle))
            return work;
        return await _musicBrainzHttpClient.GetWorkAsync(musicBrainzWorkId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds the track of <paramref name="release"/> that matches the provided <paramref name="lookup"/>, matching the recording identifier first,
    /// then the disc and track numbers, then the title, and finally the duration.
    /// </summary>
    /// <param name="release">The release whose tracklist is searched.</param>
    /// <param name="lookup">The lookup describing the local track.</param>
    /// <returns>The matching track, or <see langword="null"/> when no track matched.</returns>
    private static MusicBrainzTrackResponse? FindTrack(MusicBrainzReleaseResponse release, TrackMetadataLookupDto lookup)
    {
        // The recording identifier is the most precise match, so it is used first, if present.
        if (lookup.MusicBrainzRecordingId is Guid musicBrainzRecordingId)
        {
            foreach (MusicBrainzMediumResponse medium in release.Media)
                foreach (MusicBrainzTrackResponse track in medium.Tracks)
                    if (track.Recording?.Id is string recordingId && Guid.TryParse(recordingId, out Guid trackRecordingId) && trackRecordingId == musicBrainzRecordingId)
                        return track;
        }

        List<MusicBrainzTrackResponse> candidates = [];
        foreach (MusicBrainzMediumResponse medium in release.Media)
        {
            if (lookup.DiscNumber is int discNumber && medium.Position != discNumber)
                continue;
            foreach (MusicBrainzTrackResponse track in medium.Tracks)
            {
                if (lookup.TrackNumber is int trackNumber && track.Position != trackNumber)
                    continue;
                candidates.Add(track);
            }
        }
        if (candidates.Count <= 1)
            return candidates.Count == 1 ? candidates[0] : null;

        // The disc and track numbers were not enough to single the track out, so the title is used next, and the duration breaks any remaining tie.
        if (!string.IsNullOrWhiteSpace(lookup.Title))
        {
            List<MusicBrainzTrackResponse> titledCandidates = [.. candidates.Where(candidate => string.Equals(candidate.Title, lookup.Title, StringComparison.OrdinalIgnoreCase))];
            if (titledCandidates.Count == 1)
                return titledCandidates[0];
            if (titledCandidates.Count > 1)
                candidates = titledCandidates;
        }
        if (candidates.Count > 1 && lookup.DurationInSeconds is int duration)
            return candidates.OrderBy(candidate => Math.Abs((candidate.Length ?? 0) / 1000 - duration)).First();
        return candidates[0];
    }

    /// <summary>
    /// Gets the work the provided <paramref name="recording"/> is a performance of, from the relations already embedded in the response.
    /// </summary>
    /// <param name="recording">The recording whose work is resolved.</param>
    /// <returns>The embedded work, or <see langword="null"/> when the recording has no work.</returns>
    private static MusicBrainzWorkResponse? ResolveInlineWork(MusicBrainzRecordingResponse recording)
    {
        return recording.Relations
            .Where(relation => string.Equals(relation.TargetType, "work", StringComparison.OrdinalIgnoreCase))
            .Select(relation => relation.Work)
            .FirstOrDefault(work => work is not null);
    }

    /// <summary>
    /// Resolves the full recording of the provided search or ISRC lookup result.
    /// </summary>
    /// <param name="recording">The partial recording to resolve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The full recording, or <see langword="null"/> when the recording could not be resolved.</returns>
    private async Task<MusicBrainzRecordingResponse?> ResolveRecordingAsync(MusicBrainzRecordingResponse? recording, CancellationToken cancellationToken)
    {
        if (recording?.Id is null || !Guid.TryParse(recording.Id, out Guid recordingId))
            return recording;

        return await _musicBrainzHttpClient.GetRecordingAsync(recordingId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the work the provided recording is an instance of, together with its credited contributors.
    /// </summary>
    /// <param name="recording">The recording whose work is resolved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The work of the recording, or <see langword="null"/> when the recording has no work.</returns>
    private async Task<MusicBrainzWorkResponse?> ResolveWorkAsync(MusicBrainzRecordingResponse recording, CancellationToken cancellationToken)
    {
        string? workId = recording.Relations
            .Where(relation => string.Equals(relation.TargetType, "work", StringComparison.OrdinalIgnoreCase))
            .Select(relation => relation.Work?.Id)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        if (workId is null || !Guid.TryParse(workId, out Guid parsedWorkId))
            return recording.Relations
                .Where(relation => string.Equals(relation.TargetType, "work", StringComparison.OrdinalIgnoreCase))
                .Select(relation => relation.Work)
                .FirstOrDefault(work => work is not null);

        return await _musicBrainzHttpClient.GetWorkAsync(parsedWorkId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the release the provided recording appears on, for the release level language and script.
    /// </summary>
    /// <param name="recording">The recording whose release is resolved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The release of the recording, or <see langword="null"/> when the recording does not reference a release.</returns>
    private async Task<MusicBrainzReleaseResponse?> ResolveReleaseAsync(MusicBrainzRecordingResponse recording, CancellationToken cancellationToken)
    {
        string? releaseId = recording.Releases.Select(release => release.Id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        if (releaseId is null || !Guid.TryParse(releaseId, out Guid parsedReleaseId))
            return null;

        return await _musicBrainzHttpClient.GetReleaseAsync(parsedReleaseId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the MusicBrainz recording search query for the provided lookup.
    /// </summary>
    /// <param name="lookup">The lookup describing the track.</param>
    /// <returns>The built search query.</returns>
    private static string BuildRecordingQuery(TrackMetadataLookupDto lookup)
    {
        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(lookup.Title))
            parts.Add($"recording:\"{lookup.Title.Trim().Replace("\"", string.Empty, StringComparison.Ordinal)}\"");
        if (!string.IsNullOrWhiteSpace(lookup.ArtistName))
            parts.Add($"artist:\"{lookup.ArtistName.Trim().Replace("\"", string.Empty, StringComparison.Ordinal)}\"");
        if (!string.IsNullOrWhiteSpace(lookup.ReleaseName))
            parts.Add($"release:\"{lookup.ReleaseName.Trim().Replace("\"", string.Empty, StringComparison.Ordinal)}\"");
        return string.Join(" AND ", parts);
    }
}
