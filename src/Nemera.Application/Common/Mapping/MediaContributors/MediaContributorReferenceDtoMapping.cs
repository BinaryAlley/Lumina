#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using AudioMediaContributorId = Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate.MediaContributorId;
using BookMediaContributorId = Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate.MediaContributorId;
#endregion

namespace Lumina.Application.Common.Mapping.MediaContributors;

/// <summary>
/// Extension methods for converting <see cref="MediaContributorReferenceDto"/>.
/// </summary>
public static class MediaContributorReferenceDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to a domain <see cref="BookMediaContributor"/>.
    /// </summary>
    /// <param name="dto">The data transfer object to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="BookMediaContributor"/>, or an error message.
    /// </returns>
    public static Result<BookMediaContributor> ToBookDomainEntity(this MediaContributorReferenceDto dto)
    {
        return BookMediaContributor.Create(BookMediaContributorId.Create(dto.ContributorId), dto.Role);
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="BookMediaContributor"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="BookMediaContributor"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<BookMediaContributor>> ToBookDomainEntities(this IEnumerable<MediaContributorReferenceDto> dtos)
    {
        return dtos.Select(domainEntity => domainEntity.ToBookDomainEntity());
    }

    /// <summary>
    /// Converts <paramref name="dto"/> to a domain <see cref="MusicMediaContributor"/>.
    /// </summary>
    /// <param name="dto">The data transfer object to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="MusicMediaContributor"/>, or an error message.
    /// </returns>
    public static Result<MusicMediaContributor> ToMusicDomainEntity(this MediaContributorReferenceDto dto)
    {
        return MusicMediaContributor.Create(AudioMediaContributorId.Create(dto.ContributorId), dto.Role);
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="MusicMediaContributor"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="MusicMediaContributor"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<MusicMediaContributor>> ToMusicDomainEntities(this IEnumerable<MediaContributorReferenceDto> dtos)
    {
        return dtos.Select(domainEntity => domainEntity.ToMusicDomainEntity());
    }
}
