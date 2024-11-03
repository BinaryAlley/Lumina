#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Contracts.Entities.FileSystemManagement;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Extension methods for converting <see cref="FileSystemItem"/>.
/// </summary>
public static class FileSystemItemMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="FileSystemItemEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted entity.</returns>
    public static FileSystemItemEntity ToFileSystemItemEntity(this FileSystemItem domainEntity)
    {
        return new FileSystemItemEntity(
            domainEntity.Id.Path,
            domainEntity.Name,
            DateTime.Now,
            DateTime.Now
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted entity.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided parameter is not an expected descendant of <see cref="FileSystemItem"/>.</exception>
    public static FileSystemTreeNodeResponse ToTreeNodeResponse(this FileSystemItem domainEntity)
    {
        if (domainEntity is Directory directory)
            return directory.ToFileSystemTreeNodeResponse();
        else if (domainEntity is File file)
            return file.ToFileSystemTreeNodeResponse();
        else if (domainEntity is WindowsRootItem windowsRootItem)
            return windowsRootItem.ToTreeNodeResponse();
        else if (domainEntity is UnixRootItem unixRootItem)
            return unixRootItem.ToTreeNodeResponse();
        else
            throw new ArgumentException("Invalid FileSystemItem");
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to <see cref="IEnumerable<FileSystemTreeNodeResponse>"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted entities.</returns>
    public static IEnumerable<FileSystemTreeNodeResponse> ToTreeNodeResponses(this IEnumerable<FileSystemItem> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToTreeNodeResponse());
    }
}
