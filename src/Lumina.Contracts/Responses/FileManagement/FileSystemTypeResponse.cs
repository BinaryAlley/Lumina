#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response containing the file system platform type.
/// </summary>
/// <param name="PlatformType">The returned file system platform type.</param>
[DebuggerDisplay("{PlatformType}")]
public record FileSystemTypeResponse(
    PlatformType PlatformType
);