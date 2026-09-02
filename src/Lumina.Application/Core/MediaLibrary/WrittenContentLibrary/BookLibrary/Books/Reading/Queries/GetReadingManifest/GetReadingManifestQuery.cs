#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;

/// <summary>
/// Query for getting the reading manifest of a book.
/// </summary>
/// <param name="BookId">The Id of the book whose reading manifest is retrieved.</param>
public record GetReadingManifestQuery(
    Guid BookId
) : IQuery;
