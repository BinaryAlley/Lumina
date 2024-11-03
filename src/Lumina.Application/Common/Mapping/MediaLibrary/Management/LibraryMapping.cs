#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.MediaLibrary.Management;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="Library"/>.
/// </summary>
public static class LibraryMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="LibraryEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static LibraryEntity ToRepositoryEntity(this Library domainEntity)
    {
        return new LibraryEntity
        {
            Id = domainEntity.Id.Value,
            Title = domainEntity.Title,
            LibraryType = domainEntity.LibraryType,
            UserId = domainEntity.UserId.Value,
            ContentLocations = domainEntity.ContentLocations.Select(path => new LibraryContentLocationEntity() { Path = path.Path }).ToList(),
            Created = domainEntity.Created,
            Updated = domainEntity.Updated.HasValue ? domainEntity.Updated : null
        };
    }
}
