#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;

/// <summary>
/// Entity representing a generic file system element.
/// </summary>
[DebuggerDisplay("{Name}")]
public abstract class FileSystemItem : AggregateRoot<FileSystemPathId>
{
    /// <summary>
    /// Gets or sets the name of the file system item.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets or sets the optional parent of the file system item.
    /// </summary>
    public Optional<FileSystemItem> Parent { get; protected set; }

    /// <summary>
    /// Gets or sets the current status of the file system item. Defaults to Accessible.
    /// </summary>
    public FileSystemItemStatus Status { get; protected set; } = FileSystemItemStatus.Accessible;

    /// <summary>
    /// Gets or sets the tpe of the file system item.
    /// </summary>
    public FileSystemItemType Type { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemItem"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the file system path.</param>
    /// <param name="name">The name of the file system item.</param>
    /// <param name="fileSystemItemType">The type of the file system item.</param>
    protected FileSystemItem(FileSystemPathId id, string name, FileSystemItemType fileSystemItemType) : base(id)
    {
        Name = name;
        Type = fileSystemItemType;
    }

    /// <summary>
    /// Sets the status of the filesystem item.
    /// </summary>
    /// <param name="status">The status to be set.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public ErrorOr<Updated> SetStatus(FileSystemItemStatus status)
    {
        Status = status;
        return Result.Updated;
    }

    /// <summary>
    /// Sets the parent of the filesystem item.
    /// </summary>
    /// <param name="parent">The file system item to be set as parent.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public ErrorOr<Updated> SetParent(FileSystemItem parent)
    {
        if (parent is not null)
            Parent = parent;
        else
            return Errors.FileSystemManagement.ParentNodeCannotBeNull;
        return Result.Updated;
    }
}
