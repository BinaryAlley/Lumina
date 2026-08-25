#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for the <see cref="BookLiteDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLiteDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BookLiteDto"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the book.</param>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="releaseYear">Optional. The release year of the book.</param>
    /// <param name="coverPath">Optional. The path of the image representing the cover of the book.</param>
    /// <param name="includeReleaseYear">Whether the release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="BookLiteDto"/>.</returns>
    public BookLiteDto Create(
        Guid? id = null,
        string? title = null,
        int? releaseYear = null,
        string? coverPath = null,
        bool includeReleaseYear = false,
        bool includeCoverPath = false)
    {
        return new BookLiteDto
        {
            Id = id ?? _faker.Random.Guid(),
            Title = title ?? _faker.Lorem.Sentence(3),
            ReleaseYear = includeReleaseYear ? (releaseYear ?? _faker.Random.Int(1900, 2024)) : null,
            CoverPath = includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="BookLiteDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookLiteDto"/> instances.</returns>
    public List<BookLiteDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
