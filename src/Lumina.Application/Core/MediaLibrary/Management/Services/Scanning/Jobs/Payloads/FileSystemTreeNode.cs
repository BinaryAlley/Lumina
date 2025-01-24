#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.FileSystem;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Payloads;

/// <summary>
/// Generic file system node.
/// </summary>
[DebuggerDisplay("Path: {Path}")]
public class FileSystemTreeNode
{
    /// <summary>
    /// Gets or sets the full path of the file or directory.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the file or directory.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of the item, indicating whether it is a file, directory, or drive.
    /// </summary>
    public FileSystemItemType ItemType { get; set; }

    /// <summary>
    /// Gets or sets the list of child nodes.
    /// </summary>
    public List<FileSystemTreeNode> Children { get; set; } = [];
}
