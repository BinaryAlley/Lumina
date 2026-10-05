#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="LibraryPathPart"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathPartFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryPathPart"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of the path part.</param>
    /// <param name="representation">Optional. The literal text or the value mask of the path part. When omitted, a representation valid for the resolved kind is generated.</param>
    /// <param name="isOptional">Whether the path part can be absent from the path.</param>
    /// <returns>The created <see cref="LibraryPathPart"/>.</returns>
    public LibraryPathPart Create(
        LibraryPathPartKind? kind = null,
        string? representation = null,
        bool isOptional = false)
    {
        LibraryPathPartKind resolvedKind = kind ?? _faker.PickRandom<LibraryPathPartKind>();
        string resolvedRepresentation = representation ?? CreateValidRepresentation(resolvedKind);
        Result<LibraryPathPart> partResult = LibraryPathPart.Create(resolvedKind, resolvedRepresentation, isOptional);
        if (partResult.IsFailure)
            throw new InvalidOperationException("Failed to create LibraryPathPart: " + string.Join(", ", partResult.Errors));
        return partResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryPathPart"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryPathPart"/> instances.</returns>
    public List<LibraryPathPart> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a representation that is valid for the provided <paramref name="kind"/>.
    /// </summary>
    /// <param name="kind">The kind of the path part a representation is created for.</param>
    /// <returns>The created representation.</returns>
    private string CreateValidRepresentation(LibraryPathPartKind kind)
    {
        // A separator renders the platform path separator, so it never carries a representation.
        if (kind == LibraryPathPartKind.Separator)
            return string.Empty;
        // A literal is arbitrary on-disk text between value parts, so it carries no value placeholder.
        if (kind == LibraryPathPartKind.Literal)
            return _faker.Lorem.Word();
        // A typed part must carry exactly one value placeholder, so that the position of the captured value is unambiguous.
        return "{0}";
    }
}
