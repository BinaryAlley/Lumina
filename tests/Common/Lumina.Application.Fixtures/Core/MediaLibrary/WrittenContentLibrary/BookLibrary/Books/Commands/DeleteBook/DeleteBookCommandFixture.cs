#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;

/// <summary>
/// Fixture class for the <see cref="DeleteBookCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteBookCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="DeleteBookCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the book belongs to, taken from the route.</param>
    /// <param name="bookId">Optional. The Id of the book to delete, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBookId">Whether the book Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeleteBookCommand"/>.</returns>
    public DeleteBookCommand Create(
        string? libraryId = null,
        string? bookId = null,
        bool includeLibraryId = true,
        bool includeBookId = true)
    {
        return new DeleteBookCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeBookId ? (bookId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteBookCommand"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteBookCommand"/> instances.</returns>
    public List<DeleteBookCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
