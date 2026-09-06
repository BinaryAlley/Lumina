#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for generating <see cref="BookSeriesDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookSeriesDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="BookSeriesDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="title">Optional. The title of the book series.</param>
    /// <returns>A configured <see cref="BookSeriesDto"/> instance.</returns>
    public BookSeriesDto Create(string? title = null)
    {
        return new BookSeriesDto
        {
            Title = title ?? _faker.Lorem.Sentence(3)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="BookSeriesDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookSeriesDto"/> instances.</returns>
    public List<BookSeriesDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
