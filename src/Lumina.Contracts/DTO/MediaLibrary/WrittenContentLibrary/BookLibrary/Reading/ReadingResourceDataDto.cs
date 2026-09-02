#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Data transfer object for the binary data of a reading resource.
/// </summary>
/// <param name="Data">The binary data of the resource.</param>
/// <param name="MimeType">The MIME type of the resource.</param>
[DebuggerDisplay("MimeType: {MimeType}")]
public sealed record ReadingResourceDataDto(
    byte[] Data,
    string MimeType
);
