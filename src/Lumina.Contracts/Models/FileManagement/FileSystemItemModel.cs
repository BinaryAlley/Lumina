#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.FileManagement;

/// <summary>
/// Generic filesystem data transfer object.
/// </summary>
/// <param name="Path">The path of the file system item.</param>
/// <param name="Name">The name of the file system item.</param>
/// <param name="DateCreated">The creation date of the file system item.</param>
/// <param name="DateModified">The modification date of the file system item.</param>
[DebuggerDisplay("{Name}")]
public record FileSystemItemModel(string Path, string Name, DateTime DateCreated, DateTime DateModified);