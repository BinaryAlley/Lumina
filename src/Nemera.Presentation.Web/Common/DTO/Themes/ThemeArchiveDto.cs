#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for an in-memory ZIP archive of an installed theme.
/// </summary>
/// <param name="FileName">The file name to expose when downloading the archive.</param>
/// <param name="Content">The ZIP archive stream positioned at its start.</param>
[DebuggerDisplay("FileName: {FileName}")]
public sealed record ThemeArchiveDto(
    string FileName,
    Stream Content
);
