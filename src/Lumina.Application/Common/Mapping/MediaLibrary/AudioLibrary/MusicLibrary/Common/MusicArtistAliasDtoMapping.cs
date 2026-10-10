#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="MusicArtistAliasDto"/>.
/// </summary>
public static class MusicArtistAliasDtoMapping
{
    /// <summary>
    /// Converts <paramref name="aliasDto"/> to a domain <see cref="MusicArtistAlias"/>.
    /// </summary>
    /// <param name="aliasDto">The alias data transfer object to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the converted alias, or an error message.
    /// </returns>
    public static Result<MusicArtistAlias> ToDomainValueObject(this MusicArtistAliasDto aliasDto)
    {
        return MusicArtistAlias.Create(
            aliasDto.Name!,
            Optional<string>.FromNullable(aliasDto.SortName),
            Optional<string>.FromNullable(aliasDto.Type),
            Optional<string>.FromNullable(aliasDto.Locale),
            aliasDto.IsPrimary,
            Optional<DateOnly>.FromNullable(aliasDto.BeginDate),
            Optional<DateOnly>.FromNullable(aliasDto.EndDate),
            aliasDto.IsEnded);
    }

    /// <summary>
    /// Converts <paramref name="aliasDtos"/> to a collection of domain <see cref="MusicArtistAlias"/>.
    /// </summary>
    /// <param name="aliasDtos">The alias data transfer objects to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the converted aliases, or an error message.
    /// </returns>
    public static Result<List<MusicArtistAlias>> ToDomainValueObjects(this IEnumerable<MusicArtistAliasDto>? aliasDtos)
    {
        List<MusicArtistAlias> aliases = [];
        foreach (MusicArtistAliasDto aliasDto in aliasDtos ?? [])
        {
            Result<MusicArtistAlias> aliasResult = aliasDto.ToDomainValueObject();
            if (aliasResult.IsFailure)
                return aliasResult.Errors;
            aliases.Add(aliasResult.Value);
        }
        return aliases;
    }
}
