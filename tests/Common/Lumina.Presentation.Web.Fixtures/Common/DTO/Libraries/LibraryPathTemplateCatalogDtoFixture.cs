#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for the <see cref="LibraryPathTemplateCatalogDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplateCatalogDtoFixture
{
    private readonly LibraryPathPartDefinitionDtoFixture _libraryPathPartDefinitionDtoFixture = new();
    private readonly LibraryPathTemplatePartDtoFixture _libraryPathTemplatePartDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="LibraryPathTemplateCatalogDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="parts">Optional path parts that can be used in the path template of the library type.</param>
    /// <param name="defaultTemplateParts">Optional ordered parts of the default path template of the library type.</param>
    /// <param name="includeParts">Whether the catalog should carry selectable path parts. When <see langword="false"/>, the parts are forced to an empty collection.</param>
    /// <param name="includeDefaultTemplateParts">Whether the catalog should carry default template parts. When <see langword="false"/>, the default template parts are forced to an empty collection.</param>
    /// <returns>A configured <see cref="LibraryPathTemplateCatalogDto"/> instance.</returns>
    public LibraryPathTemplateCatalogDto Create(
        List<LibraryPathPartDefinitionDto>? parts = null,
        List<LibraryPathTemplatePartDto>? defaultTemplateParts = null,
        bool includeParts = true,
        bool includeDefaultTemplateParts = true)
    {
        return new LibraryPathTemplateCatalogDto
        {
            Parts = includeParts ? (parts ?? _libraryPathPartDefinitionDtoFixture.CreateMany(2)) : [],
            DefaultTemplateParts = includeDefaultTemplateParts ? (defaultTemplateParts ?? _libraryPathTemplatePartDtoFixture.CreateMany(2)) : []
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryPathTemplateCatalogDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryPathTemplateCatalogDto"/> instances.</returns>
    public List<LibraryPathTemplateCatalogDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
