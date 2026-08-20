#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Themes;

/// <summary>
/// Represents a theme archive response.
/// </summary>
/// <param name="Bytes">The bytes of the ZIP archive.</param>
/// <param name="FileName">The file name to expose when downloading the archive.</param>
/// <param name="ContentType">The MIME content type of the archive.</param>
[DebuggerDisplay("FileName: {FileName}")]
public record ThemeArchiveResponse(
    byte[] Bytes,
    string FileName,
    string ContentType
);
