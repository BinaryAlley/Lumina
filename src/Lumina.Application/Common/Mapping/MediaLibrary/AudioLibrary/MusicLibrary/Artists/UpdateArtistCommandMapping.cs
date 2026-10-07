#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
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
        MusicArtistMetadataDto metadata = command.Metadata!;

        // Map the media contributors that make up the artist to their domain counterparts.
        IEnumerable<Result<MusicMediaContributor>> domainContributorsResult = (command.Contributors ?? []).ToMusicDomainEntities();
        List<Error> errors = [.. domainContributorsResult.Where(contributorResult => contributorResult.IsFailure).SelectMany(contributorResult => contributorResult.Errors)];
        if (errors.Count > 0)
            return errors;

        // Build each album of the artist, together with its metadata, contributors, ratings and tracks.
        IEnumerable<Result<Album>> domainAlbumsResult = command.Albums!.ToDomainEntities();
        errors = [.. domainAlbumsResult.Where(albumResult => albumResult.IsFailure).SelectMany(albumResult => albumResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<Genre>> domainGenresResult = (metadata.Genres ?? []).ToDomainValueObjects();
        errors = [.. domainGenresResult.Where(genreResult => genreResult.IsFailure).SelectMany(genreResult => genreResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<Tag>> domainTagsResult = (metadata.Tags ?? []).ToDomainValueObjects();
        errors = [.. domainTagsResult.Where(tagResult => tagResult.IsFailure).SelectMany(tagResult => tagResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<AudioRating>> domainRatingsResult = (command.Ratings ?? []).ToDomainValueObjects();
        errors = [.. domainRatingsResult.Where(ratingResult => ratingResult.IsFailure).SelectMany(ratingResult => ratingResult.Errors)];
        if (errors.Count > 0)
            return errors;

        Result<Optional<MusicArea>> areaResult = metadata.Area.ToDomainValueObject();
        if (areaResult.IsFailure)
            return areaResult.Errors;
        Result<Optional<MusicArea>> beginAreaResult = metadata.BeginArea.ToDomainValueObject();
        if (beginAreaResult.IsFailure)
            return beginAreaResult.Errors;
        Result<Optional<MusicArea>> endAreaResult = metadata.EndArea.ToDomainValueObject();
        if (endAreaResult.IsFailure)
            return endAreaResult.Errors;

        Result<List<MusicArtistAlias>> aliasesResult = metadata.Aliases.ToDomainValueObjects();
        if (aliasesResult.IsFailure)
            return aliasesResult.Errors;

        Optional<MusicBrainzId> musicBrainzArtistId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzArtistId is not null)
            musicBrainzArtistId = MusicBrainzId.Create(command.MusicBrainzArtistId.Value);

        // The details and the media library collections of the artist are replaced in place, preserving its identity and creation metadata.
        Result<Updated> updateDetailsResult = artist.UpdateDetails(
            metadata.Name!,
            Optional<string>.FromNullable(metadata.SortName),
            Optional<string>.FromNullable(metadata.Disambiguation),
            Optional<MusicArtistType>.FromNullable(metadata.Type),
            Optional<MusicArtistGender>.FromNullable(metadata.Gender),
            Optional<string>.FromNullable(metadata.Country),
            areaResult.Value,
            beginAreaResult.Value,
            endAreaResult.Value,
            Optional<DateOnly>.FromNullable(metadata.LifeSpanBegin),
            Optional<DateOnly>.FromNullable(metadata.LifeSpanEnd),
            metadata.IsEnded,
            Optional<string>.FromNullable(command.Website),
            musicBrainzArtistId);
        if (updateDetailsResult.IsFailure)
            return updateDetailsResult.Errors;

        artist.UpdateAliases(aliasesResult.Value);
        artist.UpdateIpis([.. (command.Ipis ?? [])]);
        artist.UpdateIsnis([.. (command.Isnis ?? [])]);
        artist.UpdateGenres([.. domainGenresResult.Select(genreResult => genreResult.Value)]);
        artist.UpdateTags([.. domainTagsResult.Select(tagResult => tagResult.Value)]);
        artist.UpdateRatings([.. domainRatingsResult.Select(ratingResult => ratingResult.Value)]);
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
                domainAlbum.Disambiguation,
                domainAlbum.MediaFormat,
                domainAlbum.Packaging,
                domainAlbum.Script,
                domainAlbum.Barcode,
                [.. domainAlbum.CatalogNumbers],
                domainAlbum.Label,
                domainAlbum.ASIN,
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
                    domainTrack.IsVideo,
                    domainTrack.Work,
                    domainTrack.MusicBrainzRecordingId,
                    domainTrack.MusicBrainzTrackId,
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
