#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="ParsedLibraryPath"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class ParsedLibraryPathFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ParsedLibraryPath"/>.
    /// </summary>
    /// <param name="values">Optional. The captured values, keyed by their part kind.</param>
    /// <param name="includeValues">Whether the parsed path should carry captured values. When <see langword="false"/>, an empty set of values is created.</param>
    /// <returns>The created <see cref="ParsedLibraryPath"/>.</returns>
    public ParsedLibraryPath Create(
        IReadOnlyDictionary<LibraryPathPartKind, string>? values = null,
        bool includeValues = true)
    {
        if (!includeValues)
            return ParsedLibraryPath.Create(new Dictionary<LibraryPathPartKind, string>());
        return ParsedLibraryPath.Create(values ?? CreateRandomValues());
    }

    /// <summary>
    /// Creates multiple <see cref="ParsedLibraryPath"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ParsedLibraryPath"/> instances.</returns>
    public List<ParsedLibraryPath> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a bounded set of random captured values.
    /// </summary>
    /// <returns>The captured values, keyed by their part kind.</returns>
    private Dictionary<LibraryPathPartKind, string> CreateRandomValues()
    {
        LibraryPathPartKind[] candidateKinds =
        [
            LibraryPathPartKind.Artist,
            LibraryPathPartKind.ReleaseName,
            LibraryPathPartKind.TrackName,
            LibraryPathPartKind.Title,
            LibraryPathPartKind.Author
        ];

        Dictionary<LibraryPathPartKind, string> values = [];
        foreach (LibraryPathPartKind kind in candidateKinds.Take(_faker.Random.Int(1, candidateKinds.Length)))
            values[kind] = _faker.Lorem.Word();
        return values;
    }
}
