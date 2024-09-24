#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;

/// <summary>
/// Entity representing a file system file.
/// </summary>
[DebuggerDisplay("{Id.Path}")]
public sealed class File : FileSystemItem
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the creation date of the file system item. Can be optional if the information is not available.
    /// </summary>
    public Optional<DateTime> DateCreated { get; private set; }

    /// <summary>
    /// Gets or sets the last modification date of the file system item. Can be optional if the information is not available.
    /// </summary>
    public Optional<DateTime> DateModified { get; private set; }

    /// <summary>
    /// Gets or sets the size in bytes of the current file entity.
    /// </summary>
    public long Size { get; private set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="File"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the file in the file system path.</param>
    /// <param name="name">The name of the file.</param>
    /// <param name="dateCreated">The date and time the file was created. Can be optional if unknown.</param>
    /// <param name="dateModified">The date and time the file was last modified. Can be optional if unknown.</param>
    /// <param name="size">The size of the file in bytes.</param>
    private File(FileSystemPathId id, string name, Optional<DateTime> dateCreated, Optional<DateTime> dateModified, long size) : base(id, name, FileSystemItemType.File)
    {
        DateCreated = dateCreated;
        DateModified = dateModified;
        Size = size;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="File"/> class.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="dateCreated">The date and time the file was created. Can be optional if unknown.</param>
    /// <param name="dateModified">The date and time the file was last modified. Can be optional if unknown.</param>
    /// <param name="size">The size of the file in bytes.</param>
    /// <param name="status">The status of the file system item.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="File"/>, or an error message.
    /// </returns>
    public static ErrorOr<File> Create(
        string path,
        string name,
        Optional<DateTime> dateCreated,
        Optional<DateTime> dateModified,
        long size,
        FileSystemItemStatus status = FileSystemItemStatus.Accessible)
    {
        // TODO: enforce invariants
        ErrorOr<FileSystemPathId> createPathResult = FileSystemPathId.Create(path);
        if (createPathResult.IsError)
            return createPathResult.Errors;
        File newFile = new(
            createPathResult.Value,
            name,
            dateCreated,
            dateModified,
            size);
        ErrorOr<Updated> setStatusResult = newFile.SetStatus(status);
        if (setStatusResult.IsError)
            return setStatusResult.Errors;
        return newFile;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="File"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the file in the file system path.</param>
    /// <param name="name">The name of the file.</param>
    /// <param name="dateCreated">The date and time the file was created. Can be optional if unknown.</param>
    /// <param name="dateModified">The date and time the file was last modified. Can be optional if unknown.</param>
    /// <param name="size">The size of the file in bytes.</param>
    /// <param name="status">The status of the file system item.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="File"/>, or an error message.
    /// </returns>
    public static ErrorOr<File> Create(
        FileSystemPathId id,
        string name,
        Optional<DateTime> dateCreated,
        Optional<DateTime> dateModified,
        long size,
        FileSystemItemStatus status = FileSystemItemStatus.Accessible)
    {
        // TODO: enforce invariants        
        File newFile = new(
            id,
            name,
            dateCreated,
            dateModified,
            size);
        ErrorOr<Updated> setStatusResult = newFile.SetStatus(status);
        if (setStatusResult.IsError)
            return setStatusResult.Errors;
        return newFile;
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
    /// Updates the size of the file.
    /// </summary>
    /// <param name="newSize">The new size, in bytes.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public ErrorOr<Updated> UpdateSize(long newSize)
    {
        Size = newSize;
        return Result.Updated;
    }

    /// <summary>
    /// Renames the file system item to the specified new name.
    /// </summary>
    /// <param name="newName">The new name for the file system item.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public ErrorOr<Updated> Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Errors.FileManagement.NameCannotBeEmpty;
        Name = newName;
        return Result.Updated;
    }
    #endregion
}
