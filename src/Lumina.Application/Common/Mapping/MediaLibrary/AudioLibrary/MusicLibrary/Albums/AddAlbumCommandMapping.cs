#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
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
/// Extension methods for converting <see cref="AddAlbumCommand"/>.
/// </summary>
public static class AddAlbumCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to a domain <see cref="Album"/>.
    /// </summary>
    /// <param name="command">The command whose data is used to create the album.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Album"/>, or an error message.
    /// </returns>
    public static Result<Album> ToDomainEntity(this AddAlbumCommand command)
    {
        Result<AlbumMetadata> metadataResult = command.Metadata!.ToDomainEntity();
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        IEnumerable<Result<MusicMediaContributor>> domainContributorsResult = command.Contributors!.ToMusicDomainEntities();
        List<Error> errors = [.. domainContributorsResult.Where(contributorResult => contributorResult.IsFailure).SelectMany(contributorResult => contributorResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<AudioRating>> domainRatingsResult = command.Ratings!.ToDomainEntities();
        errors = [.. domainRatingsResult.Where(ratingResult => ratingResult.IsFailure).SelectMany(ratingResult => ratingResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<Track>> domainTracksResult = command.Tracks!.ToDomainEntities();
        errors = [.. domainTracksResult.Where(trackResult => trackResult.IsFailure).SelectMany(trackResult => trackResult.Errors)];
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
            metadataResult.Value,
            Optional<MusicMediaFormat>.FromNullable(command.MediaFormat),
            barcode,
            Optional<string>.FromNullable(command.CatalogNumber),
            musicBrainzReleaseId,
            musicBrainzReleaseGroupId,
            musicBrainzReleaseArtistId,
            [.. domainContributorsResult.Select(contributorResult => contributorResult.Value)],
            [.. domainRatingsResult.Select(ratingResult => ratingResult.Value)],
            [.. domainTracksResult.Select(trackResult => trackResult.Value)]);
    }

    /// <summary>
    /// Converts <paramref name="commands"/> to a collection of domain <see cref="Album"/>.
    /// </summary>
    /// <param name="commands">The commands whose data is used to create the albums.</param>
    /// <returns>A collection of <see cref="Result{TValue}"/> containing either the converted albums, or error messages.</returns>
    public static IEnumerable<Result<Album>> ToDomainEntities(this IEnumerable<AddAlbumCommand> commands)
    {
        return commands.Select(command => command.ToDomainEntity());
    }
}
