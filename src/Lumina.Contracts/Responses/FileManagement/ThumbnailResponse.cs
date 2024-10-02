#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.PhotoLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Data transfer object for thumbnails.
/// </summary>
/// <param name="Type">The type of the thumbnail image.</param>
/// <param name="Bytes">The bytes representing the thumbail.</param>
[DebuggerDisplay("{Type}")]
public record ThumbnailResponse(ImageType Type, byte[] Bytes);
