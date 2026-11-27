#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object carrying the data needed by the deleted items page of a book library.
/// </summary>
/// <param name="Library">The media library whose deleted books are shown.</param>
/// <param name="DeletedBooks">The deleted books of the media library, ordered by the time they were deleted, most recent first.</param>
public sealed record DeletedBooksPageDto(
    LibraryDto Library,
    IReadOnlyList<DeletedBookDto> DeletedBooks
);
