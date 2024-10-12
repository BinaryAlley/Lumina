#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.ValueObjects.Fixtures;

/// <summary>
/// Fixture class for the <see cref="PathSegment"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSegmentFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathSegmentFixture"/> class.
    /// </summary>
    public PathSegmentFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="PathSegment"/>.
    /// </summary>
    /// <returns>The created <see cref="PathSegment"/>.</returns>
    public PathSegment CreatePathSegment(
       string? name = null,
       bool? isDirectory = null,
       bool? isDrive = null)
    {
        name ??= _faker.System.FileName();
        isDirectory ??= true;
        isDrive ??= false;

        ErrorOr<PathSegment> pathResult = PathSegment.Create(name, isDirectory.Value, isDrive.Value);

        if (pathResult.IsError)
            throw new InvalidOperationException("Failed to create PathSegment: " + string.Join(", ", pathResult.Errors));
        return pathResult.Value;
    }

    /// <summary>
    /// Creates a list of <see cref="PathSegment"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PathSegment> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => CreatePathSegment()).ToList();
    }
}
