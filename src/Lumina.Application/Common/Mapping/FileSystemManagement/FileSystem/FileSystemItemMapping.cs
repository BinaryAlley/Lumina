#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Contracts.Entities.FileSystemManagement;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
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
    /// Converts <paramref name="domainModel"/> to <see cref="FileSystemItemEntity"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted model.</returns>
    public static FileSystemItemEntity ToFileSystemItemModel(this FileSystemItem domainModel)
    {
        return new FileSystemItemEntity(
            domainModel.Id.Path,
            domainModel.Name,
            DateTime.Now,
            DateTime.Now
        );
    }

    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted model.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided parameter is not an expected descendant of <see cref="FileSystemItem"/>.</exception>
    public static FileSystemTreeNodeResponse ToTreeNodeResponse(this FileSystemItem domainModel)
    {
        if (domainModel is Directory directory)
            return directory.ToFileSystemTreeNodeResponse();
        else if (domainModel is File file)
            return file.ToFileSystemTreeNodeResponse();
        else if (domainModel is WindowsRootItem windowsRootItem)
            return windowsRootItem.ToTreeNodeResponse();
        else if (domainModel is UnixRootItem unixRootItem)
            return unixRootItem.ToTreeNodeResponse();
        else
            throw new ArgumentException("Invalid FileSystemItem");
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to <see cref="IEnumerable<FileSystemTreeNodeResponse>"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted models.</returns>
    public static IEnumerable<FileSystemTreeNodeResponse> ToTreeNodeResponses(this IEnumerable<FileSystemItem> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToTreeNodeResponse());
    }
}
