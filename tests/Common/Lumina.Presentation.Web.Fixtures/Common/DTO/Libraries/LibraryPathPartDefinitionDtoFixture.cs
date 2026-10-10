#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for the <see cref="LibraryPathPartDefinitionDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathPartDefinitionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="LibraryPathPartDefinitionDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="kind">Optional kind of the path part.</param>
    /// <param name="valueType">Optional value type captured by the path part.</param>
    /// <param name="defaultRepresentation">Optional default representation of the path part.</param>
    /// <param name="isOptionalByDefault">Optional value indicating whether the path part is optional by default.</param>
    /// <returns>A configured <see cref="LibraryPathPartDefinitionDto"/> instance.</returns>
    public LibraryPathPartDefinitionDto Create(
        string? kind = null,
        string? valueType = null,
        string? defaultRepresentation = null,
        bool? isOptionalByDefault = null)
    {
        return new LibraryPathPartDefinitionDto
        {
            Kind = kind ?? _faker.Random.Word(),
            ValueType = valueType ?? _faker.Random.Word(),
            DefaultRepresentation = defaultRepresentation ?? "{0}",
            IsOptionalByDefault = isOptionalByDefault ?? _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryPathPartDefinitionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryPathPartDefinitionDto"/> instances.</returns>
    public List<LibraryPathPartDefinitionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
