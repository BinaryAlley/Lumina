#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="BookLiteRow"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLiteRowFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BookLiteRow"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the book.</param>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="releaseYear">Optional. The release year of the book.</param>
    /// <param name="coverPath">Optional. The path of the cover image of the book.</param>
    /// <returns>The created <see cref="BookLiteRow"/>.</returns>
    public BookLiteRow Create(
        Guid? id = null,
        string? title = null,
        int? releaseYear = null,
        string? coverPath = null)
    {
        return new BookLiteRow
        {
            Id = id ?? _faker.Random.Guid(),
            Title = title ?? _faker.Lorem.Sentence(),
            ReleaseYear = releaseYear,
            CoverPath = coverPath
        };
    }

    /// <summary>
    /// Creates a list of <see cref="BookLiteRow"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookLiteRow"/> instances.</returns>
    public List<BookLiteRow> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
