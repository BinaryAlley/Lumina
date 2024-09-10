#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;

/// <summary>
/// Entity representing a file system file.
/// </summary>
[DebuggerDisplay("{Id.Path}")]
public class File : FileSystemItem
{
    #region ==================================================================== PROPERTIES =================================================================================
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
    /// <param name="dateCreated">The date and time the file was created. Can be <see langword="null"/> if unknown.</param>
    /// <param name="dateModified">The date and time the file was last modified. Can be <see langword="null"/> if unknown.</param>
    /// <param name="size">The size of the file in bytes.</param>
    public File(FileSystemPathId id, string name, DateTime? dateCreated, DateTime? dateModified, long size) : base(id, name, dateCreated, dateModified)
    {
        Size = size;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Updates the size of the file.
    /// </summary>
    /// <param name="newSize">The new size, in bytes.</param>
    public void UpdateSize(long newSize)
    {
        Size = newSize;
    }
    #endregion
}