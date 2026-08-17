#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.FileSystemManagement;

/// <summary>
/// Data transfer object for a model for storing binary data (blob).
/// </summary>
[DebuggerDisplay("ContentType: {ContentType} (Data.Length)")]
public class BlobDataDto
{
    /// <summary>
    /// Gets or sets the binary data as a byte array.
    /// </summary>
    public byte[] Data { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content type (MIME type) of the blob data, such as "image/png" for an image or "application/pdf" for a PDF document.
    /// </summary>
    public string ContentType { get; set; } = null!;
}
