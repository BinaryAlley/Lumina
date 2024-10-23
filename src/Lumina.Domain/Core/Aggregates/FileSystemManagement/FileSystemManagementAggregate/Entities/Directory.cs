#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;

/// <summary>
/// Entity representing a file system directory.
/// </summary>
[DebuggerDisplay("{Id.Path}")]
public sealed class Directory : FileSystemItem
{
    private readonly List<FileSystemItem> _items = [];

    /// <summary>
    /// Gets or sets the creation date of the file system item. Can be optional if the information is not available.
    /// </summary>
    public Optional<DateTime> DateCreated { get; private set; }

    /// <summary>
    /// Gets or sets the last modification date of the file system item. Can be optional if the information is not available.
    /// </summary>
    public Optional<DateTime> DateModified { get; private set; }

    /// <summary>
    /// Gets the collection of file system items that are children to the current directory entity.
    /// </summary>
    public IReadOnlyCollection<FileSystemItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Directory"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the directory in the file system path.</param>
    /// <param name="name">The name of the directory.</param>
    /// <param name="dateCreated">The date and time the directory was created. Can be optional if unknown.</param>
    /// <param name="dateModified">The date and time the directory was last modified. Can be optional if unknown.</param>
    private Directory(FileSystemPathId id, string name, Optional<DateTime> dateCreated, Optional<DateTime> dateModified) : base(id, name, FileSystemItemType.Directory)
    {
        DateCreated = dateCreated;
        DateModified = dateModified;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Directory"/> class.
    /// </summary>
    /// <param name="name">The name of the directory.</param>
    /// <param name="dateCreated">The date and time the directory was created. Can be optional if unknown.</param>
    /// <param name="dateModified">The date and time the directory was last modified. Can be optional if unknown.</param>
    /// <param name="status">The status of the file system item.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Directory"/>, or an error message.
    /// </returns>
    public static ErrorOr<Directory> Create(
        string path,
        string name,
        Optional<DateTime> dateCreated,
        Optional<DateTime> dateModified,
        FileSystemItemStatus status = FileSystemItemStatus.Accessible)
    {
        // TODO: enforce invariants
        ErrorOr<FileSystemPathId> createPathResult = FileSystemPathId.Create(path);
        if (createPathResult.IsError)
            return createPathResult.Errors;
        Directory newDirectory = new(
            createPathResult.Value,
            name,
            dateCreated,
            dateModified);
        ErrorOr<Updated> setStatusResult = newDirectory.SetStatus(status);
        if (setStatusResult.IsError)
            return setStatusResult.Errors;
        return newDirectory;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Directory"/> class.
    /// </summary>
    /// <param name="name">The name of the directory.</param>
    /// <param name="dateCreated">The date and time the directory was created. Can be optional if unknown.</param>
    /// <param name="dateModified">The date and time the directory was last modified. Can be optional if unknown.</param>
    /// <param name="status">The status of the file system item.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Directory"/>, or an error message.
    /// </returns>
    public static ErrorOr<Directory> Create(
        FileSystemPathId id,
        string name,
        Optional<DateTime> dateCreated,
        Optional<DateTime> dateModified,
        FileSystemItemStatus status = FileSystemItemStatus.Accessible)
    {
        // TODO: enforce invariants        
        Directory newDirectory = new(
            id,
            name,
            dateCreated,
            dateModified);
        ErrorOr<Updated> setStatusResult = newDirectory.SetStatus(status);
        if (setStatusResult.IsError)
            return setStatusResult.Errors;
        return newDirectory;
    }

    /// <summary>
    /// Updates the last modified date of the file system item.
    /// </summary>
    /// <param name="date">The new date and time of last modification.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public ErrorOr<Updated> UpdateLastModified(DateTime date)
    {
        DateModified = date;
        return Result.Updated;
    }

    /// <summary>
    /// Adds a file system item to the directory.
    /// </summary>
    /// <param name="item">The file system item to add.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public ErrorOr<Updated> AddItem(FileSystemItem item)
    {
        _items.Add(item);
        return Result.Updated;
    }

    /// <summary>
    /// Removes a file system item from the directory.
    /// </summary>
    /// <param name="item">The file system item to remove.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public ErrorOr<Updated> RemoveItem(FileSystemItem item)
    {
        _items.Remove(item);
        return Result.Updated;
    }
}
