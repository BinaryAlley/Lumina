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
/// Fixture class for the <see cref="LibraryPathPartDefinition"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathPartDefinitionFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryPathPartDefinition"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of the described part.</param>
    /// <param name="valueType">Optional. The value type captured by the described part.</param>
    /// <param name="defaultRepresentation">Optional. The default representation of the described part.</param>
    /// <param name="isOptionalByDefault">Whether the described part is optional by default.</param>
    /// <returns>The created <see cref="LibraryPathPartDefinition"/>.</returns>
    public LibraryPathPartDefinition Create(
        LibraryPathPartKind? kind = null,
        LibraryPathValueType? valueType = null,
        string? defaultRepresentation = null,
        bool isOptionalByDefault = false)
    {
        return new LibraryPathPartDefinition(
            kind ?? _faker.PickRandom<LibraryPathPartKind>(),
            valueType ?? _faker.PickRandom<LibraryPathValueType>(),
            defaultRepresentation ?? "{0}",
            isOptionalByDefault);
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryPathPartDefinition"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryPathPartDefinition"/> instances.</returns>
    public List<LibraryPathPartDefinition> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
