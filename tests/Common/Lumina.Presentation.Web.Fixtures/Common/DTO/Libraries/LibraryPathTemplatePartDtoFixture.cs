#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for the <see cref="LibraryPathTemplatePartDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplatePartDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="LibraryPathTemplatePartDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="kind">Optional kind of the path part.</param>
    /// <param name="representation">Optional literal text or value mask of the path part.</param>
    /// <param name="isOptional">Optional value indicating whether the path part can be absent from the path.</param>
    /// <returns>A configured <see cref="LibraryPathTemplatePartDto"/> instance.</returns>
    public LibraryPathTemplatePartDto Create(
        string? kind = null,
        string? representation = null,
        bool? isOptional = null)
    {
        return new LibraryPathTemplatePartDto
        {
            Kind = kind ?? _faker.Random.Word(),
            Representation = representation ?? "{0}",
            IsOptional = isOptional ?? _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryPathTemplatePartDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryPathTemplatePartDto"/> instances.</returns>
    public List<LibraryPathTemplatePartDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
