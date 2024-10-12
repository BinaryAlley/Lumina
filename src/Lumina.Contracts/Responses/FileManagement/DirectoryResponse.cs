#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Models.FileManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response containing a file system directory.
/// </summary>
/// <param name="Path">The path of the directory.</param>
/// <param name="Name">The name of the directory.</param>
/// <param name="DateCreated">The creation date of the directory.</param>
/// <param name="DateModified">The modification date of the directory.</param>
/// <param name="Items">The children items of the directory.</param>
[DebuggerDisplay("{Name} (Directory)")]
public record DirectoryResponse(
    string Path, 
    string Name, 
    DateTime DateCreated, 
    DateTime DateModified, 
    List<FileSystemItemModel> Items
) : FileSystemItemModel(Path, Name, DateCreated, DateModified);
