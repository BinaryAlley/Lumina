#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Generic file system node.
/// </summary>
[DebuggerDisplay("{Path}")]
public class FileSystemTreeNodeResponse
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
    /// Gets or sets a value indicating whether the node is expanded. 
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the child nodes have been loaded.
    /// </summary>
    public bool ChildrenLoaded { get; set; }

    /// <summary>
    /// Gets or sets the list of child nodes.
    /// </summary>
    public List<FileSystemTreeNodeResponse> Children { get; set; } = [];
}