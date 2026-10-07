#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
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
/// Extension methods for converting <see cref="AddArtistCommand"/>.
/// </summary>
public static class AddArtistCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to <see cref="Artist"/>.
    /// </summary>
    /// <param name="command">The command to be converted.</param>
    /// <param name="libraryId">The Id of the media library the artist is added to.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> ToDomainEntity(this AddArtistCommand command, Guid libraryId)
    {
        MusicArtistMetadataDto metadata = command.Metadata!;

        // Map the media contributors referenced by the user to their domain counterparts; a contributor is identified by its id and carries the role it played.
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

        return Artist.Create(
            LibraryId.Create(libraryId),
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
            musicBrainzArtistId,
            [.. (command.Ipis ?? [])],
            [.. (command.Isnis ?? [])],
            aliasesResult.Value,
            [.. domainGenresResult.Select(genreResult => genreResult.Value)],
            [.. domainTagsResult.Select(tagResult => tagResult.Value)],
            [.. domainRatingsResult.Select(ratingResult => ratingResult.Value)],
            [.. domainContributorsResult.Select(contributorResult => contributorResult.Value)],
            [.. domainAlbumsResult.Select(albumResult => albumResult.Value)]);
    }
}
