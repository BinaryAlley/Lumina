#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.FileSystemManagement;

/// <summary>
/// Model containing the file system platform type.
/// </summary>`
[DebuggerDisplay("{PlatformType}")]
public class FileSystemTypeModel
{
    /// <summary>
    /// Gets or sets the file system platform type.
    /// </summary>
    public PlatformType PlatformType { get; set; }
}