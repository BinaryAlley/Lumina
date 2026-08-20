#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.Infrastructure.Models.DTO.Themes;

/// <summary>
/// Data transfer object for a downloadable archive of a theme pack.
/// </summary>
/// <param name="Bytes">The bytes of the ZIP archive.</param>
/// <param name="FileName">The file name to expose when downloading the archive.</param>
/// <param name="ContentType">The MIME content type of the archive.</param>
[DebuggerDisplay("FileName: {FileName}")]
public sealed record ThemeArchiveDto(
    byte[] Bytes,
    string FileName,
    string ContentType
);
