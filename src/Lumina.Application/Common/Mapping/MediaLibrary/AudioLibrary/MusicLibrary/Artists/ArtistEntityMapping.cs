#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
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
            Optional<string>.FromNullable(repositoryEntity.Website),
            musicBrainzArtistId,
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
        return new ArtistResponse(
            repositoryEntity.Id,
            repositoryEntity.LibraryId,
            repositoryEntity.Name,
            repositoryEntity.Website,
            repositoryEntity.MusicBrainzArtistId,
            [.. repositoryEntity.Contributors.Select(contributor => new MediaContributorReferenceDto(contributor.MediaContributorId, contributor.Role))],
            [.. repositoryEntity.Albums.Select(album => album.ToResponse())],
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.UpdatedOnUtc);
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
