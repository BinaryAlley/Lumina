#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Models.FileManagement;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response containing a file system file.
/// </summary>
/// <param name="Path">The path of the file.</param>
/// <param name="Name">The name of the file.</param>
/// <param name="DateCreated">The creation date of the file.</param>
/// <param name="DateModified">The modification date of the file.</param>
/// <param name="Size">The size of the file, in bytes.</param>
[DebuggerDisplay("{Name} (File, Size: {Size} bytes)")]
public record FileResponse(string Path, string Name, DateTime DateCreated, DateTime DateModified, long Size) : FileSystemItemModel(Path, Name, DateCreated, DateModified);