#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Extension methods for converting <see cref="UnixRootItem"/>.
/// </summary>
public static class UnixRootItemMapping
{
    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileSystemTreeNodeResponse ToTreeNodeResponse(this UnixRootItem domainModel)
    {
        return new FileSystemTreeNodeResponse()
        {
            Path = domainModel.Id.Path,
            Name = domainModel.Name,
            ItemType = FileSystemItemType.Root,
            IsExpanded = false,
            ChildrenLoaded = false,
            Children = domainModel.Items.Select(item => item.ToTreeNodeResponse())
                                        .ToList()
        };
    }
}
