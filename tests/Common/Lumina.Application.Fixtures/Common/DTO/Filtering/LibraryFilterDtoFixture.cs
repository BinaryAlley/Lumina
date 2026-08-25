#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Filtering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Filtering;

/// <summary>
/// Fixture class for the <see cref="LibraryFilterDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryFilterDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryFilterDto"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library used to filter data.</param>
    /// <param name="searchTerm">Optional. The search term used to filter results.</param>
    /// <param name="filterAlphaKey">Optional. The alpha key used to filter by the first character of the item title.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the leading "The " prefix should be ignored when computing the alpha key, or not.</param>
    /// <returns>The created <see cref="LibraryFilterDto"/>.</returns>
    public LibraryFilterDto Create(
        Guid? libraryId = null,
        string? searchTerm = null,
        string? filterAlphaKey = null,
        bool shouldIgnoreThePrefixForAlphaPicker = false)
    {
        return new LibraryFilterDto
        {
            LibraryId = libraryId ?? _faker.Random.Guid(),
            SearchTerm = searchTerm,
            FilterAlphaKey = filterAlphaKey,
            ShouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker
        };
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryFilterDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryFilterDto"/> instances.</returns>
    public List<LibraryFilterDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
