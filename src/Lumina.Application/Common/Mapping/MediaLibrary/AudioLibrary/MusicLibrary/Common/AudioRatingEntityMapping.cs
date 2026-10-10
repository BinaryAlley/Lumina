#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="AudioRatingEntity"/>.
/// </summary>
public static class AudioRatingEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="AudioRatingDto"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted DTO.</returns>
    public static AudioRatingDto ToResponse(this AudioRatingEntity repositoryEntity)
    {
        return new AudioRatingDto(
            repositoryEntity.Value ?? default,
            repositoryEntity.MaxValue ?? default,
            repositoryEntity.Source,
            repositoryEntity.VoteCount
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="AudioRatingDto"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted DTOs.</returns>
    public static IEnumerable<AudioRatingDto> ToResponses(this IEnumerable<AudioRatingEntity> repositoryEntities)
    {
        return repositoryEntities.Select(repositoryEntity => repositoryEntity.ToResponse());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="AudioRating"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="AudioRating"/>, or an error message.
    /// </returns>
    public static Result<AudioRating> ToDomainValueObject(this AudioRatingEntity repositoryEntity)
    {
        return AudioRating.Create(
            repositoryEntity.Value ?? default,
            repositoryEntity.MaxValue ?? default,
            Optional<AudioRatingSource>.FromNullable(repositoryEntity.Source),
            Optional<int>.FromNullable(repositoryEntity.VoteCount)
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="AudioRating"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="AudioRating"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<AudioRating>> ToDomainValueObjects(this IEnumerable<AudioRatingEntity> repositoryEntities)
    {
        return repositoryEntities.Select(repositoryEntity => repositoryEntity.ToDomainValueObject());
    }
}
