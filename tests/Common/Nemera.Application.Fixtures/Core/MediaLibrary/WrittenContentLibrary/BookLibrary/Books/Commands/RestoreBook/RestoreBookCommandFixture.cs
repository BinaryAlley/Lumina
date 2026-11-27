#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.RestoreBook;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.RestoreBook;

/// <summary>
/// Fixture class for the <see cref="RestoreBookCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreBookCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="RestoreBookCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the book belongs to, taken from the route.</param>
    /// <param name="bookId">Optional. The Id of the deleted book to restore, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBookId">Whether the book Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="RestoreBookCommand"/>.</returns>
    public RestoreBookCommand Create(
        string? libraryId = null,
        string? bookId = null,
        bool includeLibraryId = true,
        bool includeBookId = true)
    {
        return new RestoreBookCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeBookId ? (bookId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="RestoreBookCommand"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RestoreBookCommand"/> instances.</returns>
    public List<RestoreBookCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
