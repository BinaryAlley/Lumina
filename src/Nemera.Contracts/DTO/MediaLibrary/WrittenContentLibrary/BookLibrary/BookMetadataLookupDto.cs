#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for a book metadata lookup.
/// </summary>
/// <param name="LibraryId">The Id of the media library the book belongs to.</param>
/// <param name="Path">The file system path of the book.</param>
/// <param name="Isbn">The ISBN (International Standard Book Number) of the book, if applicable.</param>
/// <param name="OpenLibraryId">The Open Library ID of the book, if applicable.</param>
/// <param name="Title">The title of the book, if applicable.</param>
/// <param name="Author">The author of the book, if applicable.</param>
/// <param name="LanguageCode">The language code of the book, if applicable.</param>
public sealed record BookMetadataLookupDto(
    Guid LibraryId,
    string Path,
    string? Isbn = null,
    string? OpenLibraryId = null,
    string? Title = null,
    string? Author = null,
    string? LanguageCode = null
) : MetadataLookupDto;
