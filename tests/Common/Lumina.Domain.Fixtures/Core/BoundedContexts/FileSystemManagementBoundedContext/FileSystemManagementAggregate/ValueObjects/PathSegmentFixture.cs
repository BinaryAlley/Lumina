#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="PathSegment"/> value object.
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
    /// <param name="name">Optional. The segment name.</param>
    /// <param name="isDirectory">Whether the segment is a directory.</param>
    /// <param name="isDrive">Whether the segment is a drive.</param>
    /// <returns>The created <see cref="PathSegment"/>.</returns>
    public PathSegment Create(
        string? name = null,
        bool? isDirectory = null,
        bool? isDrive = null)
    {
        name ??= _faker.System.FileName();
        isDirectory ??= true;
        isDrive ??= false;

        Result<PathSegment> pathResult = PathSegment.Create(name, isDirectory.Value, isDrive.Value);

        if (pathResult.IsFailure)
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
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
