#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Nemera.Contracts.Responses.FileSystemManagement.Common;
using Nemera.Contracts.Responses.FileSystemManagement.Directories;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Nemera.Application.Common.Mapping.FileSystemManagement.Directories;

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
            IsHidden = domainEntity.IsHidden,
            IsExpanded = false,
            ChildrenLoaded = false,
            Children = [.. domainEntity.Items.Select(item => item.ToTreeNodeResponse())]
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
            [.. domainEntity.Items.Select(fileSystemItem => fileSystemItem.ToFileSystemItemDto())],
            domainEntity.IsHidden
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
