#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="LibraryEntity"/>.
/// </summary>
public static class LibraryEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="LibraryResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response entity.</returns>
    public static LibraryResponse ToResponse(this LibraryEntity repositoryEntity)
    {
        return new LibraryResponse(
            repositoryEntity.Id,
            repositoryEntity.UserId,
            repositoryEntity.Title,
            repositoryEntity.LibraryType,
            [.. repositoryEntity.ContentLocations.Select(location => location.Path)],
            repositoryEntity.CoverImage,
            repositoryEntity.IsEnabled,
            repositoryEntity.IsLocked,
            repositoryEntity.CanDownloadMetadataFromWeb,
            repositoryEntity.ShouldSaveMetadataInMediaDirectories,
            repositoryEntity.ShouldSkipUnchangedDirectoriesDuringScan,
            [.. repositoryEntity.PathTemplateParts
                .OrderBy(part => part.Position)
                .Select(part => new LibraryPathTemplatePartResponse(part.Kind.ToString(), part.Representation, part.IsOptional))],
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.UpdatedOnUtc
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Library"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Library"/>, or an error message.
    /// </returns>
    public static Result<Library> ToDomainEntity(this LibraryEntity repositoryEntity)
    {
        Result<LibraryPathTemplate> pathTemplateResult = BuildPathTemplate(repositoryEntity.PathTemplateParts);
        if (pathTemplateResult.IsFailure)
            return pathTemplateResult.Errors;

        return Library.Create(
            LibraryId.Create(repositoryEntity.Id),
            UserId.Create(repositoryEntity.UserId),
            repositoryEntity.Title,
            repositoryEntity.LibraryType,
            repositoryEntity.ContentLocations.Select(contentLocation => contentLocation.Path),
            Optional<string>.FromNullable(repositoryEntity.CoverImage),
            repositoryEntity.IsEnabled,
            repositoryEntity.IsLocked,
            repositoryEntity.CanDownloadMetadataFromWeb,
            repositoryEntity.ShouldSaveMetadataInMediaDirectories,
            repositoryEntity.ShouldSkipUnchangedDirectoriesDuringScan,
            pathTemplateResult.Value,
            [.. repositoryEntity.LibraryScans.Select(libraryScan => ScanId.Create(libraryScan.Id))]
        );
    }

    /// <summary>
    /// Builds the path template of a media library from its ordered stored parts.
    /// </summary>
    /// <param name="parts">The stored parts of the path template.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the built <see cref="LibraryPathTemplate"/>, or an error message.
    /// </returns>
    private static Result<LibraryPathTemplate> BuildPathTemplate(IEnumerable<LibraryPathTemplatePartEntity> parts)
    {
        List<LibraryPathPart> orderedParts = [];
        foreach (LibraryPathTemplatePartEntity part in parts.OrderBy(part => part.Position))
        {
            Result<LibraryPathPart> partResult = LibraryPathPart.Create(part.Kind, part.Representation, part.IsOptional);
            if (partResult.IsFailure)
                return partResult.Errors;
            orderedParts.Add(partResult.Value);
        }
        return LibraryPathTemplate.Create(orderedParts);
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Library"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="Library"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<Library>> ToDomainEntities(this IEnumerable<LibraryEntity> repositoryEntities)
    {
        return repositoryEntities.Select(library => library.ToDomainEntity());
    }
}
