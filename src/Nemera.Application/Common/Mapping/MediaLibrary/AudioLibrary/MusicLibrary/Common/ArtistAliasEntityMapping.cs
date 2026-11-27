#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="ArtistAliasEntity"/>.
/// </summary>
public static class ArtistAliasEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="MusicArtistAlias"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="MusicArtistAlias"/>, or an error message.
    /// </returns>
    public static Result<MusicArtistAlias> ToDomainValueObject(this ArtistAliasEntity repositoryEntity)
    {
        return MusicArtistAlias.Create(
            repositoryEntity.Name,
            Optional<string>.FromNullable(repositoryEntity.SortName),
            Optional<string>.FromNullable(repositoryEntity.Type),
            Optional<string>.FromNullable(repositoryEntity.Locale),
            repositoryEntity.IsPrimary,
            Optional<DateOnly>.FromNullable(repositoryEntity.BeginDate),
            Optional<DateOnly>.FromNullable(repositoryEntity.EndDate),
            repositoryEntity.IsEnded);
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="MusicArtistAlias"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the converted aliases, or an error message.
    /// </returns>
    public static Result<List<MusicArtistAlias>> ToDomainValueObjects(this IEnumerable<ArtistAliasEntity> repositoryEntities)
    {
        List<MusicArtistAlias> aliases = [];
        foreach (ArtistAliasEntity repositoryEntity in repositoryEntities)
        {
            Result<MusicArtistAlias> aliasResult = repositoryEntity.ToDomainValueObject();
            if (aliasResult.IsFailure)
                return aliasResult.Errors;
            aliases.Add(aliasResult.Value);
        }
        return aliases;
    }
}
