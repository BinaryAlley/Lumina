#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Extension methods for converting <see cref="UnixRootItem"/>.
/// </summary>
public static class UnixRootItemMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileSystemTreeNodeResponse ToTreeNodeResponse(this UnixRootItem domainEntity)
    {
        return new FileSystemTreeNodeResponse()
        {
            Path = domainEntity.Id.Path,
            Name = domainEntity.Name,
            ItemType = FileSystemItemType.Root,
            IsExpanded = false,
            ChildrenLoaded = false,
            Children = domainEntity.Items.Select(item => item.ToTreeNodeResponse())
                                         .ToList()
        };
    }
}
