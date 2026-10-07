#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
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
/// Extension methods for converting <see cref="ArtistEntity"/>.
/// </summary>
public static class ArtistEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Artist"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> ToDomainEntity(this ArtistEntity repositoryEntity)
    {
        Optional<MusicBrainzId> musicBrainzArtistId = Optional<MusicBrainzId>.None();
        if (repositoryEntity.MusicBrainzArtistId is not null)
            musicBrainzArtistId = MusicBrainzId.Create(repositoryEntity.MusicBrainzArtistId.Value);

        Optional<MusicArea> area = Optional<MusicArea>.None();
        if (repositoryEntity.Area is not null)
        {
            Result<MusicArea> areaResult = repositoryEntity.Area.ToDomainValueObject();
            if (areaResult.IsFailure)
                return areaResult.Errors;
            area = areaResult.Value;
        }
        Optional<MusicArea> beginArea = Optional<MusicArea>.None();
        if (repositoryEntity.BeginArea is not null)
        {
            Result<MusicArea> beginAreaResult = repositoryEntity.BeginArea.ToDomainValueObject();
            if (beginAreaResult.IsFailure)
                return beginAreaResult.Errors;
            beginArea = beginAreaResult.Value;
        }
        Optional<MusicArea> endArea = Optional<MusicArea>.None();
        if (repositoryEntity.EndArea is not null)
        {
            Result<MusicArea> endAreaResult = repositoryEntity.EndArea.ToDomainValueObject();
            if (endAreaResult.IsFailure)
                return endAreaResult.Errors;
            endArea = endAreaResult.Value;
        }

        Result<List<MusicArtistAlias>> domainAliasesResult = repositoryEntity.Aliases.ToDomainValueObjects();
        if (domainAliasesResult.IsFailure)
            return domainAliasesResult.Errors;
        List<MusicArtistAlias> domainAliases = domainAliasesResult.Value;

        List<Genre> domainGenres = [];
        foreach (Result<Genre> genreResult in repositoryEntity.Genres.ToDomainValueObjects())
        {
            if (genreResult.IsFailure)
                return genreResult.Errors;
            domainGenres.Add(genreResult.Value);
        }

        List<Tag> domainTags = [];
        foreach (Result<Tag> tagResult in repositoryEntity.Tags.ToDomainValueObjects())
        {
            if (tagResult.IsFailure)
                return tagResult.Errors;
            domainTags.Add(tagResult.Value);
        }

        List<AudioRating> domainRatings = [];
        foreach (Result<AudioRating> ratingResult in repositoryEntity.Ratings.ToDomainValueObjects())
        {
            if (ratingResult.IsFailure)
                return ratingResult.Errors;
            domainRatings.Add(ratingResult.Value);
        }

        List<MusicMediaContributor> domainContributors = [];
        foreach (ArtistContributorEntity contributorEntity in repositoryEntity.Contributors)
        {
            Result<MusicMediaContributor> contributorResult = MusicMediaContributor.Create(MediaContributorId.Create(contributorEntity.MediaContributorId), contributorEntity.Role);
            if (contributorResult.IsFailure)
                return contributorResult.Errors;
            domainContributors.Add(contributorResult.Value);
        }

        List<Album> domainAlbums = [];
        foreach (AlbumEntity albumEntity in repositoryEntity.Albums)
        {
            Result<Album> albumResult = albumEntity.ToDomainEntity();
            if (albumResult.IsFailure)
                return albumResult.Errors;
            domainAlbums.Add(albumResult.Value);
        }

        return Artist.Create(
            ArtistId.Create(repositoryEntity.Id),
            LibraryId.Create(repositoryEntity.LibraryId),
            repositoryEntity.Name,
            Optional<string>.FromNullable(repositoryEntity.SortName),
            Optional<string>.FromNullable(repositoryEntity.Disambiguation),
            Optional<MusicArtistType>.FromNullable(repositoryEntity.Type),
            Optional<MusicArtistGender>.FromNullable(repositoryEntity.Gender),
            Optional<string>.FromNullable(repositoryEntity.Country),
            area,
            beginArea,
            endArea,
            Optional<DateOnly>.FromNullable(repositoryEntity.LifeSpanBegin),
            Optional<DateOnly>.FromNullable(repositoryEntity.LifeSpanEnd),
            repositoryEntity.IsEnded,
            Optional<string>.FromNullable(repositoryEntity.Website),
            musicBrainzArtistId,
            [.. repositoryEntity.Ipis.Select(ipi => ipi.Value)],
            [.. repositoryEntity.Isnis.Select(isni => isni.Value)],
            domainAliases,
            domainGenres,
            domainTags,
            domainRatings,
            domainContributors,
            domainAlbums,
            repositoryEntity.CreatedOnUtc,
            Optional<DateTime>.FromNullable(repositoryEntity.UpdatedOnUtc));
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="ArtistResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response entity.</returns>
    public static ArtistResponse ToResponse(this ArtistEntity repositoryEntity)
    {
        MusicArtistMetadataDto metadata = new(
            repositoryEntity.Name,
            repositoryEntity.SortName,
            repositoryEntity.Disambiguation,
            repositoryEntity.Type,
            repositoryEntity.Gender,
            repositoryEntity.Country,
            repositoryEntity.Area.ToResponse(),
            repositoryEntity.BeginArea.ToResponse(),
            repositoryEntity.EndArea.ToResponse(),
            repositoryEntity.LifeSpanBegin,
            repositoryEntity.LifeSpanEnd,
            repositoryEntity.IsEnded,
            [.. repositoryEntity.Genres.ToResponses()],
            [.. repositoryEntity.Tags.ToResponses()],
            [.. repositoryEntity.Aliases.Select(alias => new MusicArtistAliasDto(alias.Name, alias.SortName, alias.Type, alias.Locale, alias.IsPrimary, alias.BeginDate, alias.EndDate, alias.IsEnded))]);
        return new ArtistResponse(
            repositoryEntity.Id,
            repositoryEntity.LibraryId,
            metadata,
            repositoryEntity.Website,
            repositoryEntity.MusicBrainzArtistId,
            [.. repositoryEntity.Ipis.Select(ipi => ipi.Value)],
            [.. repositoryEntity.Isnis.Select(isni => isni.Value)],
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.UpdatedOnUtc,
            [.. repositoryEntity.Contributors.Select(contributor => new MediaContributorReferenceDto(contributor.MediaContributorId, contributor.Role))],
            [.. repositoryEntity.Ratings.ToResponses()],
            [.. repositoryEntity.Albums.Select(album => album.ToResponse())]);
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="ArtistResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<ArtistResponse> ToResponses(this IEnumerable<ArtistEntity> repositoryEntities)
    {
        return [.. repositoryEntities.Select(repositoryEntity => repositoryEntity.ToResponse())];
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a paginated collection of <see cref="ArtistResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The paginated repository entities to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<ArtistResponse> ToResponses(this PaginatedResultDto<ArtistEntity> repositoryEntities)
    {
        return new PaginatedResponse<ArtistResponse>
        {
            Data = repositoryEntities.Data.ToResponses(),
            CurrentPage = repositoryEntities.CurrentPage,
            PerPage = repositoryEntities.PerPage,
            Count = repositoryEntities.Count,
            NumberOfPages = repositoryEntities.NumberOfPages
        };
    }
}
