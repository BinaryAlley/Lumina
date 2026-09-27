#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="UpdateArtistCommand"/>.
/// </summary>
public static class UpdateArtistCommandMapping
{
    /// <summary>
    /// Applies the editable data of <paramref name="command"/> to <paramref name="artist"/>, through the aggregate root.
    /// </summary>
    /// <param name="command">The command whose data is applied to the artist.</param>
    /// <param name="artist">The artist aggregate to be updated.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the successfully updated <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> ToDomainEntity(this UpdateArtistCommand command, Artist artist)
    {
        // Map the media contributors that make up the artist to their domain counterparts.
        IEnumerable<Result<MusicMediaContributor>> domainContributorsResult = command.Contributors!.ToMusicDomainEntities();
        List<Error> errors = [.. domainContributorsResult.Where(contributorResult => contributorResult.IsFailure).SelectMany(contributorResult => contributorResult.Errors)];
        if (errors.Count > 0)
            return errors;

        // Build each album of the artist, together with its metadata, contributors, ratings and tracks.
        IEnumerable<Result<Album>> domainAlbumsResult = command.Albums!.ToDomainEntities();
        errors = [.. domainAlbumsResult.Where(albumResult => albumResult.IsFailure).SelectMany(albumResult => albumResult.Errors)];
        if (errors.Count > 0)
            return errors;

        Optional<string> website = Optional<string>.FromNullable(command.Website);
        Optional<MusicBrainzId> musicBrainzArtistId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzArtistId is not null)
            musicBrainzArtistId = MusicBrainzId.Create(command.MusicBrainzArtistId.Value);

        // The details and the media contributors of the artist are replaced in place, preserving its identity and creation metadata.
        Result<Updated> updateDetailsResult = artist.UpdateDetails(command.Name!, website, musicBrainzArtistId);
        if (updateDetailsResult.IsFailure)
            return updateDetailsResult.Errors;
        artist.UpdateContributors([.. domainContributorsResult.Select(contributorResult => contributorResult.Value)]);

        List<Album> domainAlbums = [.. domainAlbumsResult.Select(albumResult => albumResult.Value)];

        // Reconcile the albums by their Id: existing albums and their tracks are updated in place, new albums are added, and albums that
        // are no longer present are removed. New albums are added before the stale ones are removed, because an artist must always keep at least one album.
        foreach (Album domainAlbum in domainAlbums)
        {
            // The existing album is the aggregate member, referenced by object, and is updated in place rather than being replaced.
            Album? existingAlbum = artist.Albums.FirstOrDefault(album => album.Id == domainAlbum.Id);
            if (existingAlbum is null)
            {
                Result<Created> addAlbumResult = artist.AddAlbum(domainAlbum);
                if (addAlbumResult.IsFailure)
                    return addAlbumResult.Errors;
                continue;
            }

            Result<Updated> updateAlbumResult = artist.UpdateAlbum(
                existingAlbum,
                domainAlbum.Metadata,
                domainAlbum.MediaFormat,
                domainAlbum.Barcode,
                domainAlbum.CatalogNumber,
                domainAlbum.MusicBrainzReleaseId,
                domainAlbum.MusicBrainzReleaseGroupId,
                domainAlbum.MusicBrainzReleaseArtistId,
                domainAlbum.Contributors,
                domainAlbum.Ratings);
            if (updateAlbumResult.IsFailure)
                return updateAlbumResult.Errors;

            // Reconcile the tracks by their Id: existing tracks are updated in place, new tracks are added, and tracks that are no longer present are removed.
            foreach (Track domainTrack in domainAlbum.Tracks)
            {
                Track? existingTrack = existingAlbum.Tracks.FirstOrDefault(track => track.Id == domainTrack.Id);
                if (existingTrack is null)
                {
                    Result<Created> addTrackResult = artist.AddTrackToAlbum(existingAlbum, domainTrack);
                    if (addTrackResult.IsFailure)
                        return addTrackResult.Errors;
                    continue;
                }

                Result<Updated> updateTrackResult = artist.UpdateTrackInAlbum(
                    existingAlbum,
                    existingTrack,
                    domainTrack.Path,
                    domainTrack.Metadata,
                    domainTrack.TrackNumber,
                    domainTrack.DiscNumber,
                    domainTrack.Script,
                    domainTrack.Key,
                    domainTrack.Bpm,
                    domainTrack.Work,
                    domainTrack.MusicBrainzRecordingId,
                    domainTrack.MusicBrainzTrackId,
                    domainTrack.MusicBrainzWorkId,
                    domainTrack.Moods,
                    domainTrack.Isrcs,
                    domainTrack.Contributors,
                    domainTrack.Ratings);
                if (updateTrackResult.IsFailure)
                    return updateTrackResult.Errors;
            }

            foreach (Track staleTrack in existingAlbum.Tracks.Where(existingTrack => domainAlbum.Tracks.All(domainTrack => domainTrack.Id != existingTrack.Id)).ToList())
            {
                Result<Deleted> removeTrackResult = artist.RemoveTrackFromAlbum(existingAlbum, staleTrack);
                if (removeTrackResult.IsFailure)
                    return removeTrackResult.Errors;
            }
        }

        foreach (Album staleAlbum in artist.Albums.Where(existingAlbum => domainAlbums.All(domainAlbum => domainAlbum.Id != existingAlbum.Id)).ToList())
        {
            Result<Deleted> removeAlbumResult = artist.RemoveAlbum(staleAlbum);
            if (removeAlbumResult.IsFailure)
                return removeAlbumResult.Errors;
        }

        return artist;
    }
}
