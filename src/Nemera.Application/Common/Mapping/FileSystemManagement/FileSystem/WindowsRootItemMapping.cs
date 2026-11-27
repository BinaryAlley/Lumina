#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.FileSystem;
using Nemera.Contracts.Responses.FileSystemManagement.Common;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
#endregion

namespace Nemera.Application.Common.Mapping.FileSystemManagement.FileSystem;

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
