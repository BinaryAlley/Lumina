#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="ArtistMetadataDto"/>.
/// </summary>
public static class ArtistMetadataDtoMapping
{
    /// <summary>
    /// Applies the metadata of <paramref name="dto"/> to <paramref name="artist"/>, through the aggregate root.
    /// </summary>
    /// <param name="dto">The artist metadata to apply.</param>
    /// <param name="artist">The artist aggregate the metadata is applied to.</param>
    /// <param name="contributors">The media contributors that make up the artist, already resolved to their persisted identifiers.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the successfully updated <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Updated> ApplyTo(this ArtistMetadataDto dto, Artist artist, IReadOnlyCollection<MusicMediaContributor> contributors)
    {
        Result<Optional<MusicArea>> areaResult = dto.Area.ToDomainValueObject();
        if (areaResult.IsFailure)
            return areaResult.Errors;
        Result<Optional<MusicArea>> beginAreaResult = dto.BeginArea.ToDomainValueObject();
        if (beginAreaResult.IsFailure)
            return beginAreaResult.Errors;
        Result<Optional<MusicArea>> endAreaResult = dto.EndArea.ToDomainValueObject();
        if (endAreaResult.IsFailure)
            return endAreaResult.Errors;
        Result<List<MusicArtistAlias>> aliasesResult = dto.Aliases.ToDomainValueObjects();
        if (aliasesResult.IsFailure)
            return aliasesResult.Errors;

        List<Genre> genres = [];
        foreach (GenreDto genre in dto.Genres ?? [])
        {
            Result<Genre> genreResult = genre.ToDomainValueObject();
            if (genreResult.IsFailure)
                return genreResult.Errors;
            genres.Add(genreResult.Value);
        }

        List<Tag> tags = [];
        foreach (TagDto tag in dto.Tags ?? [])
        {
            Result<Tag> tagResult = tag.ToDomainValueObject();
            if (tagResult.IsFailure)
                return tagResult.Errors;
            tags.Add(tagResult.Value);
        }

        List<AudioRating> ratings = [];
        foreach (AudioRatingDto rating in dto.Ratings ?? [])
        {
            Result<AudioRating> ratingResult = rating.ToDomainValueObject();
            if (ratingResult.IsFailure)
                return ratingResult.Errors;
            ratings.Add(ratingResult.Value);
        }

        Optional<MusicBrainzId> musicBrainzArtistId = Optional<MusicBrainzId>.None();
        if (dto.MusicBrainzArtistId is not null)
            musicBrainzArtistId = MusicBrainzId.Create(dto.MusicBrainzArtistId.Value);

        // The website is only replaced when the provider returned one, so a locally captured website is never cleared.
        Optional<string> website = string.IsNullOrWhiteSpace(dto.Website) ? artist.Website : Optional<string>.FromNullable(dto.Website);
        Result<Updated> updateDetailsResult = artist.UpdateDetails(
            string.IsNullOrWhiteSpace(dto.Name) ? artist.Name : dto.Name,
            Optional<string>.FromNullable(dto.SortName),
            Optional<string>.FromNullable(dto.Disambiguation),
            Optional<MusicArtistType>.FromNullable(dto.Type),
            Optional<MusicArtistGender>.FromNullable(dto.Gender),
            Optional<string>.FromNullable(dto.Country),
            areaResult.Value,
            beginAreaResult.Value,
            endAreaResult.Value,
            Optional<DateOnly>.FromNullable(dto.LifeSpanBegin),
            Optional<DateOnly>.FromNullable(dto.LifeSpanEnd),
            dto.IsEnded,
            website,
            musicBrainzArtistId);
        if (updateDetailsResult.IsFailure)
            return updateDetailsResult.Errors;

        artist.UpdateAliases(aliasesResult.Value);
        artist.UpdateIpis([.. dto.Ipis ?? []]);
        artist.UpdateIsnis([.. dto.Isnis ?? []]);
        artist.UpdateGenres(genres);
        artist.UpdateTags(tags);
        artist.UpdateRatings(ratings);
        artist.UpdateContributors(contributors);
        return Result.Updated;
    }
}
