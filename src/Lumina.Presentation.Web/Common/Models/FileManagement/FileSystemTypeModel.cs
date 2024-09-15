#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.FileManagement;

/// <summary>
/// Model containing the file system platform type.
/// </summary>
[DebuggerDisplay("{PlatformType}")]
public class FileSystemTypeModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the file system platform type.
    /// </summary>
    public PlatformType PlatformType { get; set; }
    #endregion
}