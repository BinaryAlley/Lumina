#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;

/// <summary>
/// Entity representing a file system root item on Windows platforms.
/// </summary>
[DebuggerDisplay("{Id.Path}")]
public sealed class WindowsRootItem : FileSystemItem
{
    private readonly List<FileSystemItem> _items = [];

    /// <summary>
    /// Gets the collection of file system items that are children to the current file system root entity.
    /// </summary>
    public IReadOnlyCollection<FileSystemItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsRootItem"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the root item in the file system path.</param>
    /// <param name="name">The name of the root item.</param>
    private WindowsRootItem(FileSystemPathId id, string name) : base(id, name, FileSystemItemType.Root)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="WindowsRootItem"/> class.
    /// </summary>
    /// <param name="path">The path of the file system root item.</param>
    /// <param name="name">The name of the file system root item.</param>
    /// <param name="status">The status of the file system root item.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="WindowsRootItem"/>, or an error message.
    /// </returns>
    public static ErrorOr<WindowsRootItem> Create(
        string path,
        string name,
        FileSystemItemStatus status = FileSystemItemStatus.Accessible)
    {
        // TODO: enforce invariants
        ErrorOr<FileSystemPathId> createPathResult = FileSystemPathId.Create(path);
        if (createPathResult.IsError)
            return createPathResult.Errors;
        WindowsRootItem newRoot = new(
            createPathResult.Value,
            name);
        ErrorOr<Updated> setStatusResult = newRoot.SetStatus(status);
        if (setStatusResult.IsError)
            return setStatusResult.Errors;
        return newRoot;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="WindowsRootItem"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the file system root item in the file system path.</param>
    /// <param name="name">The name of the file system root item.</param>
    /// <param name="status">The status of the file system root item.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="WindowsRootItem"/>, or an error message.
    /// </returns>
    public static ErrorOr<WindowsRootItem> Create(
        FileSystemPathId id,
        string name,
        FileSystemItemStatus status = FileSystemItemStatus.Accessible)
    {
        // TODO: enforce invariants        
        WindowsRootItem newFile = new(
            id,
            name);
        ErrorOr<Updated> setStatusResult = newFile.SetStatus(status);
        if (setStatusResult.IsError)
            return setStatusResult.Errors;
        return newFile;
    }
}
