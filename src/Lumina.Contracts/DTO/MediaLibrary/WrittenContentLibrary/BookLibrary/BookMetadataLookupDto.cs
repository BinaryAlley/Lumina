#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for a book metadata lookup.
/// </summary>
public sealed record BookMetadataLookupDto(
    Guid LibraryId,
    string Path,
    string? Isbn = null,
    string? OpenLibraryId = null,
    string? Title = null,
    string? Author = null,
    string? LanguageCode = null
    //string? Title,
    //string? OriginalTitle,
    //int? ReleaseYear,
    //string? LanguageCode,
    //IReadOnlyList<string>? Isbns,
    //string? GoodreadsId,
    //string? OpenLibraryId,
    //string? GoogleBooksId
) : MetadataLookupDto;
