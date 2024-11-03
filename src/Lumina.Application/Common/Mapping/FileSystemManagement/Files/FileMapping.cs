#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Files;

/// <summary>
/// Extension methods for converting <see cref="File"/>.
/// </summary>
public static class FileMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileSystemTreeNodeResponse ToFileSystemTreeNodeResponse(this File domainEntity)
    {
        return new FileSystemTreeNodeResponse()
        {
            Name = domainEntity.Name,
            Path = domainEntity.Id.Path,
            ItemType = domainEntity.Type,
            IsExpanded = false,
            ChildrenLoaded = false
        };
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<FileSystemTreeNodeResponse> ToFileSystemTreeNodeResponses(this IEnumerable<File> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToFileSystemTreeNodeResponse());
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="FileResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileResponse ToResponse(this File domainEntity)
    {
        return new FileResponse(
            domainEntity.Id.Path,
            domainEntity.Name,
            domainEntity.DateCreated.HasValue ? domainEntity.DateCreated.Value : default,
            domainEntity.DateModified.HasValue ? domainEntity.DateModified.Value : default,
            domainEntity.Size
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="FileResponse"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<FileResponse> ToResponses(this IEnumerable<File> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToResponse());
    }
}
