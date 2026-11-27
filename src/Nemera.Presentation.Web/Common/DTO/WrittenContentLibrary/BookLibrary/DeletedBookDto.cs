#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for a book that the user deleted and can restore.
/// </summary>
/// <param name="Id">The Id of the deleted book.</param>
/// <param name="LibraryId">The Id of the media library the deleted book belongs to.</param>
/// <param name="Title">The title of the deleted book.</param>
/// <param name="OriginalTitle">The original title of the deleted book, when it differs from the current one.</param>
/// <param name="Path">The file system path of the deleted book.</param>
/// <param name="DeletedOnUtc">The date and time when the book was deleted.</param>
/// <param name="CoverPath">The relative path of the stored cover image of the deleted book, when it has one.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record DeletedBookDto(
    Guid Id,
    Guid LibraryId,
    string Title,
    string? OriginalTitle,
    string Path,
    DateTime DeletedOnUtc,
    string? CoverPath
);
