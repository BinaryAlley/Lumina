#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for the <see cref="BookLiteResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLiteResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BookLiteResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the book.</param>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="releaseYear">Optional. The release year of the book.</param>
    /// <param name="coverPath">Optional. The path of the image representing the cover of the book.</param>
    /// <returns>The created <see cref="BookLiteResponse"/>.</returns>
    public BookLiteResponse Create(
        Guid? id = null,
        string? title = null,
        int? releaseYear = null,
        string? coverPath = null)
    {
        return new BookLiteResponse(
            id ?? Guid.NewGuid(),
            title ?? _faker.Commerce.ProductName(),
            releaseYear ?? _faker.Random.Int(1900, 2024),
            coverPath ?? _faker.System.FilePath()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="BookLiteResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<BookLiteResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
