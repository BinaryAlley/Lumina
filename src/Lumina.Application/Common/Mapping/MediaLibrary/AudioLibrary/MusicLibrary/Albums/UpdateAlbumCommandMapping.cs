#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="UpdateAlbumCommand"/>.
/// </summary>
public static class UpdateAlbumCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to a domain <see cref="Album"/>, preserving the identity and creation metadata of the stored album identified by <paramref name="existingAlbum"/>.
    /// </summary>
    /// <param name="command">The command whose data is used to create the album.</param>
    /// <param name="existingAlbum">The stored album whose identity and creation metadata are preserved.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Album"/>, or an error message.
    /// </returns>
    public static Result<Album> ToDomainEntity(this UpdateAlbumCommand command, AlbumEntity existingAlbum)
    {
        Result<AlbumMetadata> metadataResult = command.Metadata!.ToDomainEntity();
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        IEnumerable<Result<MusicMediaContributor>> domainContributorsResult = (command.Contributors ?? []).ToMusicDomainEntities();
        List<Error> errors = [.. domainContributorsResult.Where(contributorResult => contributorResult.IsFailure).SelectMany(contributorResult => contributorResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<AudioRating>> domainRatingsResult = (command.Ratings ?? []).ToDomainEntities();
        errors = [.. domainRatingsResult.Where(ratingResult => ratingResult.IsFailure).SelectMany(ratingResult => ratingResult.Errors)];
        if (errors.Count > 0)
            return errors;

        Optional<Barcode> barcode = Optional<Barcode>.None();
        if (command.Barcode is not null)
        {
            Result<Barcode> barcodeResult = Barcode.Create(command.Barcode);
            if (barcodeResult.IsFailure)
                return barcodeResult.Errors;
            barcode = barcodeResult.Value;
        }

        Optional<MusicBrainzId> musicBrainzReleaseId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzReleaseId is not null)
        {
            Result<MusicBrainzId> musicBrainzReleaseIdResult = MusicBrainzId.Create(command.MusicBrainzReleaseId.Value);
            if (musicBrainzReleaseIdResult.IsFailure)
                return musicBrainzReleaseIdResult.Errors;
            musicBrainzReleaseId = musicBrainzReleaseIdResult.Value;
        }
        Optional<MusicBrainzId> musicBrainzReleaseGroupId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzReleaseGroupId is not null)
        {
            Result<MusicBrainzId> musicBrainzReleaseGroupIdResult = MusicBrainzId.Create(command.MusicBrainzReleaseGroupId.Value);
            if (musicBrainzReleaseGroupIdResult.IsFailure)
                return musicBrainzReleaseGroupIdResult.Errors;
            musicBrainzReleaseGroupId = musicBrainzReleaseGroupIdResult.Value;
        }
        Optional<MusicBrainzId> musicBrainzReleaseArtistId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzReleaseArtistId is not null)
        {
            Result<MusicBrainzId> musicBrainzReleaseArtistIdResult = MusicBrainzId.Create(command.MusicBrainzReleaseArtistId.Value);
            if (musicBrainzReleaseArtistIdResult.IsFailure)
                return musicBrainzReleaseArtistIdResult.Errors;
            musicBrainzReleaseArtistId = musicBrainzReleaseArtistIdResult.Value;
        }

        return Album.Create(
            AlbumId.Create(existingAlbum.Id),
            metadataResult.Value,
            Optional<MusicMediaFormat>.FromNullable(command.MediaFormat),
            barcode,
            Optional<string>.FromNullable(command.CatalogNumber),
            musicBrainzReleaseId,
            musicBrainzReleaseGroupId,
            musicBrainzReleaseArtistId,
            [.. domainContributorsResult.Select(contributorResult => contributorResult.Value)],
            [.. domainRatingsResult.Select(ratingResult => ratingResult.Value)],
            [],
            existingAlbum.CreatedOnUtc,
            Optional<DateTime>.FromNullable(existingAlbum.UpdatedOnUtc));
    }
}
