#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="LibraryPathTemplate"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplateFixture
{
    private readonly Faker _faker = new();
    private readonly LibraryPathPartFixture _libraryPathPartFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryPathTemplate"/>.
    /// </summary>
    /// <param name="parts">Optional. The ordered parts that make up the path template.</param>
    /// <param name="includeParts">Whether the template should carry parts. When <see langword="false"/>, an empty template is created.</param>
    /// <returns>The created <see cref="LibraryPathTemplate"/>.</returns>
    public LibraryPathTemplate Create(
        IEnumerable<LibraryPathPart>? parts = null,
        bool includeParts = true)
    {
        if (!includeParts)
            return LibraryPathTemplate.Empty();
        return LibraryPathTemplate.Create(parts ?? _libraryPathPartFixture.CreateMany(_faker.Random.Int(1, 4)));
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryPathTemplate"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryPathTemplate"/> instances.</returns>
    public List<LibraryPathTemplate> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
