#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents the response of an update book cover operation.
/// </summary>
/// <param name="CoverPath">The relative path of the stored cover image.</param>
[DebuggerDisplay("CoverPath: {CoverPath}")]
public record UpdateBookCoverResponse(
    string CoverPath
);
