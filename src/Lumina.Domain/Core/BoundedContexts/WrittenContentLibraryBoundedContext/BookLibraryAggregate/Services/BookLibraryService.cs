#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services;

/// <summary>
/// Service for managing book libraries.
/// </summary>
public sealed class BookLibraryService : IBookLibraryService
{
    /// <summary>
    /// Gets the collection of books in the library.
    /// </summary>
    public ICollection<Book> Books => throw new NotImplementedException();
}
