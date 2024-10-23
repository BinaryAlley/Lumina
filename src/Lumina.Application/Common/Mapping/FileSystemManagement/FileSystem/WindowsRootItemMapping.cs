#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Common;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Extension methods for converting <see cref="WindowsRootItem"/>.
/// </summary>
public static class WindowsRootItemMapping
{
    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileSystemTreeNodeResponse ToTreeNodeResponse(this WindowsRootItem domainModel)
    {
        return new FileSystemTreeNodeResponse()
        {
            Path = domainModel.Id.Path,
            Name = domainModel.Name,
            ItemType = FileSystemItemType.Root,
            IsExpanded = false
        };
    }
}
