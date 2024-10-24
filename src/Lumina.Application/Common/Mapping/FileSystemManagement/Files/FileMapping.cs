#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
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
    /// Converts <paramref name="domainModel"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileSystemTreeNodeResponse ToFileSystemTreeNodeResponse(this File domainModel)
    {
        return new FileSystemTreeNodeResponse()
        {
            Name = domainModel.Name,
            Path = domainModel.Id.Path,
            ItemType = domainModel.Type,
            IsExpanded = false,
            ChildrenLoaded = false
        };
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<FileSystemTreeNodeResponse> ToFileSystemTreeNodeResponses(this IEnumerable<File> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToFileSystemTreeNodeResponse());
    }

    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="FileResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileResponse ToResponse(this File domainModel)
    {
        return new FileResponse(
            domainModel.Id.Path,
            domainModel.Name,
            domainModel.DateCreated.HasValue ? domainModel.DateCreated.Value : default,
            domainModel.DateModified.HasValue ? domainModel.DateModified.Value : default,
            domainModel.Size
        );
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="FileResponse"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<FileResponse> ToResponses(this IEnumerable<File> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToResponse());
    }
}
