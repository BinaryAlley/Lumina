#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Common;

/// <summary>
/// Extension methods for converting <see cref="FileSystemTreeNodeResponse"/>.
/// </summary>
public static class FileSystemTreeNodeResponseMapping
{
    /// <summary>
    /// Converts <paramref name="response"/> to <see cref="WindowsRootItem"/>.
    /// </summary>
    /// <param name="response">The response to be converted.</param>
    /// <returns>The converted domain model.</returns>
    public static ErrorOr<WindowsRootItem> ToWindowsRootItem(this FileSystemTreeNodeResponse response)
    {
        return WindowsRootItem.Create(
            response.Path,
            response.Name,
            FileSystemItemStatus.Accessible
        );
    }

    /// <summary>
    /// Converts <paramref name="_"/> to <see cref="UnixRootItem"/>.
    /// </summary>
    /// <param name="_">The response to be converted.</param>
    /// <returns>The converted domain model.</returns>
    public static ErrorOr<UnixRootItem> ToUnixRootItem(this FileSystemTreeNodeResponse _)
    {
        return UnixRootItem.Create(
            FileSystemItemStatus.Accessible
        );
    }
}
