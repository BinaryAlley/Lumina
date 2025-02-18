#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="LibraryScanEntity"/>.
/// </summary>
public static class LibraryScanEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="LibraryScan"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a converted <see cref="LibraryScan"/>, or an error message.
    /// </returns>
    public static ErrorOr<LibraryScan> ToDomainEntity(this LibraryScanEntity repositoryEntity)
    {
        return LibraryScan.Create(
           ScanId.Create(repositoryEntity.Id),
           LibraryId.Create(repositoryEntity.LibraryId),
           UserId.Create(repositoryEntity.UserId),
           repositoryEntity.Status,
           []
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="LibraryScan"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of converted <see cref="LibraryScan"/>, or an error message.
    /// </returns>
    public static IEnumerable<ErrorOr<LibraryScan>> ToDomainEntities(this IEnumerable<LibraryScanEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
