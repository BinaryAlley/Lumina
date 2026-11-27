#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
#endregion

namespace Nemera.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="LibraryScan"/>.
/// </summary>
public static class LibraryScanMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="LibraryScanEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static LibraryScanEntity ToRepositoryEntity(this LibraryScan domainEntity)
    {
        return new LibraryScanEntity
        {
            Id = domainEntity.Id.Value,
            UserId = domainEntity.UserId.Value,
            LibraryId = domainEntity.LibraryId.Value,
            Status = domainEntity.Status,
            User = null!,
            Library = null!
        };
    }
}
