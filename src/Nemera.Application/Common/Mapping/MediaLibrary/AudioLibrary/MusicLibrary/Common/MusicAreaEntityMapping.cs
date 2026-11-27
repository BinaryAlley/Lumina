#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
#endregion

namespace Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="MusicAreaEntity"/>.
/// </summary>
public static class MusicAreaEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="MusicArea"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="MusicArea"/>, or an error message.
    /// </returns>
    public static Result<MusicArea> ToDomainValueObject(this MusicAreaEntity repositoryEntity)
    {
        return MusicArea.Create(
            MusicBrainzId.Create(repositoryEntity.MusicBrainzAreaId),
            repositoryEntity.Name,
            Optional<string>.FromNullable(repositoryEntity.SortName),
            Optional<string>.FromNullable(repositoryEntity.Disambiguation),
            Optional<string>.FromNullable(repositoryEntity.Type),
            Optional<string>.FromNullable(repositoryEntity.Iso3166Code));
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="MusicAreaDto"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted data transfer object, or <see langword="null"/> when the repository entity is <see langword="null"/>.</returns>
    public static MusicAreaDto? ToResponse(this MusicAreaEntity? repositoryEntity)
    {
        return repositoryEntity is null
            ? null
            : new MusicAreaDto(repositoryEntity.MusicBrainzAreaId, repositoryEntity.Name, repositoryEntity.SortName, repositoryEntity.Disambiguation, repositoryEntity.Type, repositoryEntity.Iso3166Code);
    }
}
