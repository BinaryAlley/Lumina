#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Libraries;

/// <summary>
/// Represents a library element.
/// </summary>
[DebuggerDisplay("{Title}")]
public class LibraryModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the unique identifier of the library.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the library.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of the library.
    /// </summary>
    public LibraryType? LibraryType { get; set; }

    /// <summary>
    /// Gets or sets the collection of directories that contain the library files.
    /// </summary>
    public List<string> Paths { get; set; } = [];
    #endregion
}