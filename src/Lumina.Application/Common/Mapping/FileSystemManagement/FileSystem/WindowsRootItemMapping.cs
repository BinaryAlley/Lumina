#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Extension methods for converting <see cref="WindowsRootItem"/>.
/// </summary>
public static class WindowsRootItemMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="FileSystemTreeNodeResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FileSystemTreeNodeResponse ToTreeNodeResponse(this WindowsRootItem domainEntity)
    {
        return new FileSystemTreeNodeResponse()
        {
            Path = domainEntity.Id.Path,
            Name = domainEntity.Name,
            ItemType = FileSystemItemType.Root,
            IsExpanded = false
        };
    }
}
