#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Application.Common.Infrastructure.Models.MediaLibraryScanJobPayloads;

/// <summary>
/// Represents an item for the payload of the file hashing media library scan job.
/// </summary>
/// <param name="File">The file to whom <paramref name="FileHash"/> belongs.</param>
/// <param name="FileHash">The hash of <paramref name="File"/>.</param>
[DebuggerDisplay("File: {File}; FileHash: {FileHash}")]
public record ChangedFileSystemFile(FileInfo File, ulong FileHash);
