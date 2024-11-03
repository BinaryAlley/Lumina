#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Extension methods for converting <see cref="Directory"/>.
/// </summary>
public static class DirectoryMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileSystemTreeNodeResponse ToFileSystemTreeNodeResponse(this Directory domainEntity)
    {
        return new FileSystemTreeNodeResponse()
        { 
            Name = domainEntity.Name,
            Path = domainEntity.Id.Path,
            ItemType = domainEntity.Type,
            IsExpanded = false,
            ChildrenLoaded = false,
            Children = domainEntity.Items.Select(item => item.ToTreeNodeResponse())
                                         .ToList()
        };
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<FileSystemTreeNodeResponse> ToFileSystemTreeNodeResponses(this IEnumerable<Directory> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToTreeNodeResponse());
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="DirectoryResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static DirectoryResponse ToResponse(this Directory domainEntity)
    {
        return new DirectoryResponse(
            domainEntity.Id.Path,
            domainEntity.Name,
            domainEntity.DateCreated.Value,
            domainEntity.DateModified.Value,
            domainEntity.Items.Select(fileSystemItem => fileSystemItem.ToFileSystemItemEntity())
                              .ToList()
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="DirectoryResponse"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<DirectoryResponse> ToResponses(this IEnumerable<Directory> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToResponse());
    }
}
