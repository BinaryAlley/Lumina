#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Libraries;

/// <summary>
/// Data transfer object for the data of the media libraries management page.
/// </summary>
/// <param name="CanManageLibraries">Whether the current user can change and delete their media libraries, which takes the permission to manage media libraries, or the administrator role.</param>
[DebuggerDisplay("CanManageLibraries: {CanManageLibraries}")]
public sealed record LibrariesIndexPageDto(
    bool CanManageLibraries
);
