#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;

/// <summary>
/// Entity representing a file system directory.
/// </summary>
[DebuggerDisplay("{Id.Path}")]
public class Directory : FileSystemItem
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the collection of file system items that are children to the current directory entity.
    /// </summary>
    public List<FileSystemItem> Items { get; private set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="Directory"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the directory in the file system path.</param>
    /// <param name="name">The name of the directory.</param>
    /// <param name="dateCreated">The date and time the directory was created. Can be <see langword="null"/> if unknown.</param>
    /// <param name="dateModified">The date and time the directory was last modified. Can be <see langword="null"/> if unknown.</param>
    public Directory(FileSystemPathId id, string name, DateTime? dateCreated, DateTime? dateModified) : base(id, name, dateCreated, dateModified)
    {
        Items = [];
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Adds a file system item to the directory.
    /// </summary>
    /// <param name="item">The file system item to add.</param>
    public void AddItem(FileSystemItem item)
    {
        Items.Add(item);
    }

    /// <summary>
    /// Removes a file system item from the directory.
    /// </summary>
    /// <param name="item">The file system item to remove.</param>
    public void RemoveItem(FileSystemItem item)
    {
        Items.Remove(item);
    }
    #endregion
}