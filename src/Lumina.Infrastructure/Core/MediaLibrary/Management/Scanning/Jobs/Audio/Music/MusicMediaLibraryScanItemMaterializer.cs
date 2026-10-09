#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Materializer for the media library items of the music media library type.
/// </summary>
internal sealed class MusicMediaLibraryScanItemMaterializer : IMediaLibraryScanItemMaterializer
{
    private const string UNKNOWN_ARTIST_NAME = "Unknown Artist";
    private const string UNKNOWN_ALBUM_TITLE = "Unknown Album";

    /// <summary>
    /// The media library type that this materializer supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Music;

    /// <summary>
    /// Resets the enrichment state of the tracks stored at the provided <paramref name="paths"/>, together with their albums and artists.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose tracks are reset.</param>
    /// <param name="paths">The file system paths of the tracks whose enrichment state is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetEnrichmentStateForChangedPathsAsync(IUnitOfWork unitOfWork, Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken)
    {
        return await unitOfWork.TrackRepository.ResetEnrichmentStateForPathsAsync(libraryId, paths, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Materializes the artists, albums and tracks of the media library from the metadata staged for the media library scan.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose items are materialized.</param>
    /// <param name="scanId">The Id of the media library scan whose results are materialized.</param>
    /// <param name="paths">The file system paths of the media library scan snapshot.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> MaterializeItemsAsync(IUnitOfWork unitOfWork, Guid libraryId, Guid scanId, IReadOnlyList<string> paths, CancellationToken cancellationToken)
    {
        // The staged metadata of the scan holds everything needed to materialize the music library items, grouped by artist and by release.
        Result<IReadOnlyList<MusicLibraryScanItemMetadataEntity>> getStagedMetadataResult = await unitOfWork.MusicLibraryScanItemMetadataRepository.GetByScanIdAsync(scanId, cancellationToken).ConfigureAwait(false);
        if (getStagedMetadataResult.IsFailure)
            return getStagedMetadataResult.Errors;
        IReadOnlyList<MusicLibraryScanItemMetadataEntity> stagedMetadata = getStagedMetadataResult.Value;
        if (stagedMetadata.Count == 0)
            return Result.Success;

        // Tracks that are already stored are never inserted again, so a re-scan of an unchanged file does not duplicate its track.
        List<string> stagedPaths = [.. stagedMetadata.Select(stagedItem => stagedItem.Path)];
        Result<IReadOnlyCollection<string>> getExistingPathsResult = await unitOfWork.TrackRepository.GetExistingPathsAsync(libraryId, stagedPaths, cancellationToken).ConfigureAwait(false);
        if (getExistingPathsResult.IsFailure)
            return getExistingPathsResult.Errors;
        HashSet<string> existingPaths = [.. getExistingPathsResult.Value];

        // The artists of a library are uniquely identified by their name, so they are grouped by it, otherwise two groups named alike would collide with
        // the unique index of the artists; the MusicBrainz identifier of the group is the one of the release artist, because the artist of the music
        // library is the artist the release, and not the individual tracks, belong to. Albums are grouped by the on-disk directory of their tracks,
        // because the directory is the source of truth of what an album is: the tracks of one folder that disagree on their MusicBrainz release
        // identifier, or that carry none, still belong to the same album.
        foreach (IGrouping<string, MusicLibraryScanItemMetadataEntity> artistGroup in stagedMetadata.GroupBy(GetArtistName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string artistName = artistGroup.Key;
            Guid? musicBrainzArtistId = artistGroup.Select(stagedItem => stagedItem.MusicBrainzReleaseArtistId ?? stagedItem.MusicBrainzArtistId).FirstOrDefault(musicBrainzId => musicBrainzId is not null);

            Result<ArtistEntity?> getArtistResult = await unitOfWork.ArtistRepository.GetByNameAsync(libraryId, artistName, cancellationToken).ConfigureAwait(false);
            if (getArtistResult.IsFailure)
                return getArtistResult.Errors;
            ArtistEntity? existingArtist = getArtistResult.Value;

            // A brand new artist is accumulated together with all its new albums, so that the whole aggregate is inserted in a single call.
            ArtistEntity? newArtist = null;

            foreach (IGrouping<string, MusicLibraryScanItemMetadataEntity> albumGroup in artistGroup.GroupBy(stagedItem => MusicLibraryPathStructure.GetAlbumDirectory(stagedItem.Path), StringComparer.OrdinalIgnoreCase))
            {
                List<MusicLibraryScanItemMetadataEntity> albumStagedItems = [.. albumGroup];
                string albumTitle = GetAlbumTitle(albumStagedItems[0]);
                string albumDirectory = MusicLibraryPathStructure.GetAlbumDirectory(albumStagedItems[0].Path);
                Guid? musicBrainzReleaseId = albumStagedItems.Select(stagedItem => stagedItem.MusicBrainzReleaseId).FirstOrDefault(musicBrainzId => musicBrainzId is not null);
                Guid? musicBrainzReleaseGroupId = albumStagedItems.Select(stagedItem => stagedItem.MusicBrainzReleaseGroupId).FirstOrDefault(musicBrainzId => musicBrainzId is not null);
                Guid? musicBrainzReleaseArtistId = albumStagedItems.Select(stagedItem => stagedItem.MusicBrainzReleaseArtistId).FirstOrDefault(musicBrainzId => musicBrainzId is not null);

                // Only the tracks that are not stored yet are materialized, the others already have their own track row.
                List<MusicLibraryScanItemMetadataEntity> tracksToInsert = [.. albumStagedItems.Where(stagedItem => !existingPaths.Contains(stagedItem.Path))];
                if (tracksToInsert.Count == 0)
                    continue;

                if (existingArtist is null)
                {
                    newArtist ??= CreateArtistEntity(libraryId, artistName, musicBrainzArtistId);
                    AlbumEntity newAlbum = CreateAlbumEntity(libraryId, newArtist.Id, albumTitle, albumStagedItems, musicBrainzReleaseId, musicBrainzReleaseGroupId, musicBrainzReleaseArtistId);
                    foreach (MusicLibraryScanItemMetadataEntity stagedItem in tracksToInsert)
                        newAlbum.Tracks.Add(CreateTrackEntity(libraryId, newAlbum.Id, stagedItem));
                    newAlbum.TotalTracks = newAlbum.Tracks.Count;
                    newArtist.Albums.Add(newAlbum);
                }
                else
                {
                    // The directory pins the album of the artist, so releases that share a title but live in different folders are not merged.
                    Result<AlbumEntity?> getAlbumResult = await unitOfWork.AlbumRepository.GetByTrackDirectoryAsync(existingArtist.Id, albumDirectory, cancellationToken).ConfigureAwait(false);
                    if (getAlbumResult.IsFailure)
                        return getAlbumResult.Errors;
                    AlbumEntity? existingAlbum = getAlbumResult.Value;

                    // A library materialized before the directories became the grouping key is matched by title instead.
                    if (existingAlbum is null)
                    {
                        getAlbumResult = await unitOfWork.AlbumRepository.GetByTitleAsync(existingArtist.Id, albumTitle, cancellationToken).ConfigureAwait(false);
                        if (getAlbumResult.IsFailure)
                            return getAlbumResult.Errors;
                        existingAlbum = getAlbumResult.Value;
                    }

                    if (existingAlbum is null)
                    {
                        AlbumEntity newAlbum = CreateAlbumEntity(libraryId, existingArtist.Id, albumTitle, albumStagedItems, musicBrainzReleaseId, musicBrainzReleaseGroupId, musicBrainzReleaseArtistId);
                        foreach (MusicLibraryScanItemMetadataEntity stagedItem in tracksToInsert)
                            newAlbum.Tracks.Add(CreateTrackEntity(libraryId, newAlbum.Id, stagedItem));
                        newAlbum.TotalTracks = newAlbum.Tracks.Count;
                        Result<Created> insertAlbumResult = await unitOfWork.AlbumRepository.InsertAsync(newAlbum, cancellationToken).ConfigureAwait(false);
                        if (insertAlbumResult.IsFailure)
                            return insertAlbumResult.Errors;
                    }
                    else
                    {
                        foreach (MusicLibraryScanItemMetadataEntity stagedItem in tracksToInsert)
                        {
                            TrackEntity newTrack = CreateTrackEntity(libraryId, existingAlbum.Id, stagedItem);
                            Result<Created> insertTrackResult = await unitOfWork.TrackRepository.InsertAsync(newTrack, cancellationToken).ConfigureAwait(false);
                            if (insertTrackResult.IsFailure)
                                return insertTrackResult.Errors;
                        }
                    }
                }
            }

            if (newArtist is not null)
            {
                Result<Created> insertArtistResult = await unitOfWork.ArtistRepository.InsertAsync(newArtist, cancellationToken).ConfigureAwait(false);
                if (insertArtistResult.IsFailure)
                    return insertArtistResult.Errors;
            }
        }

        // The staged metadata of the scan was consumed, so it is removed, keeping the storage medium bounded across scans.
        Result<Deleted> deleteStagedMetadataResult = await unitOfWork.MusicLibraryScanItemMetadataRepository.DeleteByScanIdAsync(scanId, cancellationToken).ConfigureAwait(false);
        if (deleteStagedMetadataResult.IsFailure)
            return deleteStagedMetadataResult.Errors;

        return Result.Success;
    }

    /// <summary>
    /// Resets the metadata enrichment status of all the artists, albums and tracks of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose items are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(IUnitOfWork unitOfWork, Guid libraryId, CancellationToken cancellationToken)
    {
        Result<Updated> resetArtistsResult = await unitOfWork.ArtistRepository.ResetMetadataStatusForLibraryAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (resetArtistsResult.IsFailure)
            return resetArtistsResult.Errors;
        Result<Updated> resetAlbumsResult = await unitOfWork.AlbumRepository.ResetMetadataStatusForLibraryAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (resetAlbumsResult.IsFailure)
            return resetAlbumsResult.Errors;
        return await unitOfWork.TrackRepository.ResetMetadataStatusForLibraryAsync(libraryId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resets the artwork enrichment status of all the artists, albums and tracks of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to interact with the data access layer.</param>
    /// <param name="libraryId">The Id of the media library whose artwork is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetArtworkStatusForLibraryAsync(IUnitOfWork unitOfWork, Guid libraryId, CancellationToken cancellationToken)
    {
        return await unitOfWork.MusicArtworkRepository.ResetStatusForLibraryAsync(libraryId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the name of the artist of the provided staged <paramref name="stagedItem"/>, falling back to a placeholder when the metadata does not provide one.
    /// </summary>
    /// <param name="stagedItem">The staged music metadata item whose artist name is retrieved.</param>
    /// <returns>The name of the artist.</returns>
    private static string GetArtistName(MusicLibraryScanItemMetadataEntity stagedItem)
    {
        return string.IsNullOrWhiteSpace(stagedItem.ArtistName) ? UNKNOWN_ARTIST_NAME : stagedItem.ArtistName;
    }

    /// <summary>
    /// Gets the title of the release of the provided staged <paramref name="stagedItem"/>, falling back to a placeholder when the metadata does not provide one.
    /// </summary>
    /// <param name="stagedItem">The staged music metadata item whose release title is retrieved.</param>
    /// <returns>The title of the release.</returns>
    private static string GetAlbumTitle(MusicLibraryScanItemMetadataEntity stagedItem)
    {
        return string.IsNullOrWhiteSpace(stagedItem.ReleaseName) ? UNKNOWN_ALBUM_TITLE : stagedItem.ReleaseName;
    }

    /// <summary>
    /// Creates a new artist entity for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <param name="musicBrainzArtistId">The MusicBrainz identifier of the artist, read from the tags, if applicable.</param>
    /// <returns>The created artist entity.</returns>
    private static ArtistEntity CreateArtistEntity(Guid libraryId, string name, Guid? musicBrainzArtistId)
    {
        return new ArtistEntity
        {
            Id = Guid.NewGuid(),
            LibraryId = libraryId,
            Name = name,
            MusicBrainzArtistId = musicBrainzArtistId,
            MetadataStatus = MetadataStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a new album entity for the artist identified by <paramref name="artistId"/>, using the metadata of the provided staged <paramref name="stagedItems"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="title">The title of the album.</param>
    /// <param name="stagedItems">The staged music metadata items of the album.</param>
    /// <param name="musicBrainzReleaseId">The MusicBrainz identifier of the release, read from the tags, if applicable.</param>
    /// <param name="musicBrainzReleaseGroupId">The MusicBrainz identifier of the release group, read from the tags, if applicable.</param>
    /// <param name="musicBrainzReleaseArtistId">The MusicBrainz identifier of the release artist, read from the tags, if applicable.</param>
    /// <returns>The created album entity.</returns>
    private static AlbumEntity CreateAlbumEntity(Guid libraryId, Guid artistId, string title, IReadOnlyList<MusicLibraryScanItemMetadataEntity> stagedItems, Guid? musicBrainzReleaseId, Guid? musicBrainzReleaseGroupId, Guid? musicBrainzReleaseArtistId)
    {
        MusicLibraryScanItemMetadataEntity firstStagedItem = stagedItems[0];
        return new AlbumEntity
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            LibraryId = libraryId,
            Title = title,
            MusicBrainzReleaseId = musicBrainzReleaseId,
            MusicBrainzReleaseGroupId = musicBrainzReleaseGroupId,
            MusicBrainzReleaseArtistId = musicBrainzReleaseArtistId,
            ReleaseTypes = firstStagedItem.ReleaseType is null ? [] : [new AlbumReleaseTypeEntity(firstStagedItem.ReleaseType.Value)],
            OriginalReleaseYear = firstStagedItem.ReleaseYear,
            MetadataStatus = MetadataStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a new track entity for the album identified by <paramref name="albumId"/>, using the metadata of the provided staged <paramref name="stagedItem"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="stagedItem">The staged music metadata item of the track.</param>
    /// <returns>The created track entity.</returns>
    private static TrackEntity CreateTrackEntity(Guid libraryId, Guid albumId, MusicLibraryScanItemMetadataEntity stagedItem)
    {
        return new TrackEntity
        {
            Id = Guid.NewGuid(),
            AlbumId = albumId,
            LibraryId = libraryId,
            Path = stagedItem.Path,
            Title = string.IsNullOrWhiteSpace(stagedItem.TrackTitle) ? Path.GetFileNameWithoutExtension(stagedItem.Path) : stagedItem.TrackTitle,
            TrackNumber = stagedItem.TrackNumber ?? 0,
            DiscNumber = stagedItem.DiscNumber,
            DurationInSeconds = stagedItem.DurationInSeconds,
            SampleRate = stagedItem.SampleRate,
            Channels = stagedItem.Channels,
            BitDepth = stagedItem.BitDepth,
            AudioCodec = stagedItem.AudioCodec,
            Bitrate = stagedItem.Bitrate,
            AcoustId = stagedItem.AcoustId,
            ReplayGainTrackGain = stagedItem.ReplayGainTrackGain,
            ReplayGainTrackPeak = stagedItem.ReplayGainTrackPeak,
            ReplayGainAlbumGain = stagedItem.ReplayGainAlbumGain,
            ReplayGainAlbumPeak = stagedItem.ReplayGainAlbumPeak,
            Moods = [.. stagedItem.Moods.Select(mood => new TrackMoodEntity(mood))],
            MusicBrainzRecordingId = stagedItem.MusicBrainzRecordingId,
            MusicBrainzTrackId = stagedItem.MusicBrainzTrackId,
            MusicBrainzWorkId = stagedItem.MusicBrainzWorkId,
            WorkTitle = stagedItem.WorkTitle,
            MetadataStatus = MetadataStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
    }
}
