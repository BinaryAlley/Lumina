#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.FileSystemManagement;

/// <summary>
/// Data transfer object for a response to the inquiry about the validity of a file system path.
/// </summary>
/// <param name="IsValid">Whether a file system path is valid or not.</param>
[DebuggerDisplay("IsValid: {IsValid}")]
public record PathValidDto(
    bool IsValid
);
