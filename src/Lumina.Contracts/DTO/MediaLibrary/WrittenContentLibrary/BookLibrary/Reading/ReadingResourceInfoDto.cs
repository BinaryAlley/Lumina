#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Data transfer object for the file information of a resource of a reading document.
/// </summary>
/// <param name="RelativeFilePath">The path of the resource file, relative to the extraction directory of the book.</param>
/// <param name="MimeType">The MIME type of the resource.</param>
[DebuggerDisplay("RelativeFilePath: {RelativeFilePath}")]
public sealed record ReadingResourceInfoDto(
    string RelativeFilePath,
    string MimeType
);
