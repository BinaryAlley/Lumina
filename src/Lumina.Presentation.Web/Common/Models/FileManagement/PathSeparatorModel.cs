#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.FileManagement;

/// <summary>
/// A model representing a file system path separator.
/// </summary>
/// <param name="Separator">The system's path separator.</param>
[DebuggerDisplay("{Separator}")]
public record PathSeparatorModel(
    string Separator
);