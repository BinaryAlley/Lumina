#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Services;

/// <summary>
/// Service for managing book libraries.
/// </summary>
public sealed class BookLibraryService : IBookLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the collection of books in the library.
    /// </summary>
    public ICollection<Book> Books
    {
        get { throw new NotImplementedException(); }
    }
    #endregion
}