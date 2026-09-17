#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="WrittenContentMetadataDto"/>.
/// </summary>
public static class WrittenContentMetadataDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="WrittenContentMetadataDto"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="WrittenContentMetadata"/>, or an error message.
    /// </returns>
    public static Result<WrittenContentMetadata> ToDomainEntity(this WrittenContentMetadataDto dto)
    {

        Result<ReleaseInfo>? domainReleaseInfoResult = dto.ReleaseInfo!.ToDomainEntity();
        if (domainReleaseInfoResult.Value.IsFailure)
            return domainReleaseInfoResult.Value.Errors;

        IEnumerable<Result<Genre>> domainGenresResult = dto.Genres!.ToDomainEntities();
        List<Error> errors = [.. domainGenresResult.Where(genreResult => genreResult.IsFailure).SelectMany(genreResult => genreResult.Errors) ?? []];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<Tag>> domainTagsResult = dto.Tags!.ToDomainEntities();
        errors = [.. domainTagsResult.Where(tagResult => tagResult.IsFailure).SelectMany(tagResult => tagResult.Errors) ?? []];
        if (errors.Count > 0)
            return errors;

        Result<LanguageInfo>? domainLanguageInfoResult = dto.Language?.ToDomainEntity();
        if (domainLanguageInfoResult.HasValue && domainLanguageInfoResult.Value.IsFailure)
            return domainLanguageInfoResult.Value.Errors;

        Result<LanguageInfo>? domainOriginalLanguageInfoResult = dto.OriginalLanguage?.ToDomainEntity();
        if (domainOriginalLanguageInfoResult.HasValue && domainOriginalLanguageInfoResult.Value.IsFailure)
            return domainOriginalLanguageInfoResult.Value.Errors;

        return WrittenContentMetadata.Create(
            dto.Title!,
            Optional<string>.FromNullable(dto.OriginalTitle),
            Optional<string>.FromNullable(dto.Description),
            domainReleaseInfoResult!.Value.Value,
            [.. domainGenresResult.Select(genre => genre.Value)],
            [.. domainTagsResult.Select(tag => tag.Value)],
            Optional<LanguageInfo>.FromNullable(domainLanguageInfoResult?.Value),
            Optional<LanguageInfo>.FromNullable(domainOriginalLanguageInfoResult?.Value),
            Optional<string>.FromNullable(dto.Publisher),
            Optional<int>.FromNullable(dto.PageCount)
        );
    }
}
