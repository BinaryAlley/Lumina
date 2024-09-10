#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;

/// <summary>
/// Entity representing a generic file system element.
/// </summary>
[DebuggerDisplay("{Name}")]
public abstract class FileSystemItem : AggregateRoot<FileSystemPathId>
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the name of the file system item.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets or sets the creation date of the file system item.
    /// Can be <see langword="null"/> if the information is not available.
    /// </summary>
    public DateTime? DateCreated { get; protected set; }

    /// <summary>
    /// Gets or sets the last modification date of the file system item.
    /// Can be <see langword="null"/> if the information is not available.
    /// </summary>
    public DateTime? DateModified { get; protected set; }

    /// <summary>
    /// Gets or sets the current status of the file system item.
    /// Defaults to Accessible.
    /// </summary>
    public FileSystemItemStatus Status { get; protected set; } = FileSystemItemStatus.Accessible;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemItem"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the file system path.</param>
    /// <param name="name">The name of the file system item.</param>
    /// <param name="dateModified">The date and time the file system item was last modified. Can be <see langword="null"/> if unknown.</param>
    /// <param name="dateCreated">The date and time the file system item was created. Can be <see langword="null"/> if unknown.</param>
    protected FileSystemItem(FileSystemPathId id, string name, DateTime? dateModified, DateTime? dateCreated) : base(id)
    {
        Name = name;
        DateCreated = dateCreated;
        DateModified = dateModified;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Renames the file system item to the specified new name.
    /// </summary>
    /// <param name="newName">The new name for the file system item.</param>
    /// <exception cref="ArgumentException">Thrown if the new name is null or whitespace.</exception>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New name cannot be empty", nameof(newName));
        Name = newName;
    }

    /// <summary>
    /// Updates the last modified date of the file system item.
    /// </summary>
    /// <param name="date">The new date and time of last modification.</param>
    public void UpdateLastModified(DateTime date)
    {
        DateModified = date;
    }

    /// <summary>
    /// Sets the status of the filesystem item.
    /// </summary>
    /// <param name="status">The status to be set.</param>
    public void SetStatus(FileSystemItemStatus status)
    {
        Status = status;
    }
    #endregion
}
