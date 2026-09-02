#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Reading;

/// <summary>
/// Data transfer object for the reading availability of a book.
/// </summary>
[DebuggerDisplay("BookId: {BookId}, IsAvailable: {IsAvailable}")]
public class ReadingAvailabilityDto
{
    /// <summary>
    /// Gets or sets the Id of the book whose reading availability is reported.
    /// </summary>
    public Guid BookId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media library the book belongs to.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets whether the book can be opened for reading.
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Gets or sets the code of the error preventing the book from being read, when it cannot be read. Can be <see langword="null"/> when the book is available.
    /// </summary>
    public string? ErrorCode { get; set; }
}
