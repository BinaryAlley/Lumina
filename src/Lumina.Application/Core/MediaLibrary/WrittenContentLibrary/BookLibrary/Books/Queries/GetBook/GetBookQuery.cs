#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;

/// <summary>
/// Query for getting a book by its Id.
/// </summary>
/// <param name="Id">The Id of the book to get.</param>
public record GetBookQuery(
    Guid Id
) : IQuery;
