#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
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
    /// Converts <paramref name="domainModel"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileSystemTreeNodeResponse ToFileSystemTreeNodeResponse(this Directory domainModel)
    {
        return new FileSystemTreeNodeResponse()
        { 
            Name = domainModel.Name,
            Path = domainModel.Id.Path,
            ItemType = domainModel.Type,
            IsExpanded = false,
            ChildrenLoaded = false,
            Children = domainModel.Items.Select(item => item.ToTreeNodeResponse())
                                        .ToList()
        };
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<FileSystemTreeNodeResponse> ToFileSystemTreeNodeResponses(this IEnumerable<Directory> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToTreeNodeResponse());
    }

    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="DirectoryResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static DirectoryResponse ToResponse(this Directory domainModel)
    {
        return new DirectoryResponse(
            domainModel.Id.Path,
            domainModel.Name,
            domainModel.DateCreated.Value,
            domainModel.DateModified.Value,
            domainModel.Items.Select(fileSystemItem => fileSystemItem.ToFileSystemItemModel())
                             .ToList()
        );
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="DirectoryResponse"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<DirectoryResponse> ToResponses(this IEnumerable<Directory> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToResponse());
    }
}
