#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Diagnostics;
#endregion

namespace Nemera.Contracts.Responses.FileSystemManagement.FileSystem;

/// <summary>
/// Represents a response containing the file system platform type.
/// </summary>
/// <param name="PlatformType">The returned file system platform type.</param>
[DebuggerDisplay("PlatformType: {PlatformType}")]
public record FileSystemTypeResponse(
    PlatformType PlatformType
);
