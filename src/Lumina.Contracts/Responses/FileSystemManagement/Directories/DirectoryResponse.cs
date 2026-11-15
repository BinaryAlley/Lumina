#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.FileSystemManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileSystemManagement.Directories;

/// <summary>
/// Represents a response containing a file system directory.
/// </summary>
/// <param name="Path">The path of the directory.</param>
/// <param name="Name">The name of the directory.</param>
/// <param name="DateCreated">The creation date of the directory.</param>
/// <param name="DateModified">The modification date of the directory.</param>
/// <param name="Items">The children items of the directory.</param>
/// <param name="IsHidden">Whether the directory is hidden.</param>
[DebuggerDisplay("Name: {Name} (Directory)")]
public record DirectoryResponse(
    string Path,
    string Name,
    DateTime DateCreated,
    DateTime DateModified,
    List<FileSystemItemDto> Items,
    bool IsHidden
) : FileSystemItemDto(Path, Name, DateCreated, DateModified, IsHidden);
