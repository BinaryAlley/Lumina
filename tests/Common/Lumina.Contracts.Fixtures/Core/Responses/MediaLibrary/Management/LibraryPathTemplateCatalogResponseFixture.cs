#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryPathTemplateCatalogResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplateCatalogResponseFixture
{
    private readonly LibraryPathPartDefinitionResponseFixture _libraryPathPartDefinitionResponseFixture = new();
    private readonly LibraryPathTemplatePartResponseFixture _libraryPathTemplatePartResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryPathTemplateCatalogResponse"/>.
    /// </summary>
    /// <param name="parts">Optional. The path parts that can be used in the path template of the library type.</param>
    /// <param name="defaultTemplateParts">Optional. The ordered parts of the default path template of the library type.</param>
    /// <param name="includeParts">Whether the catalog should carry selectable path parts. When <see langword="false"/>, no parts are created.</param>
    /// <param name="includeDefaultTemplateParts">Whether the catalog should carry default template parts. When <see langword="false"/>, no default template parts are created.</param>
    /// <returns>The created <see cref="LibraryPathTemplateCatalogResponse"/>.</returns>
    public LibraryPathTemplateCatalogResponse Create(
        List<LibraryPathPartDefinitionResponse>? parts = null,
        List<LibraryPathTemplatePartResponse>? defaultTemplateParts = null,
        bool includeParts = true,
        bool includeDefaultTemplateParts = true)
    {
        return new LibraryPathTemplateCatalogResponse(
            includeParts ? (parts ?? _libraryPathPartDefinitionResponseFixture.CreateMany(2)) : [],
            includeDefaultTemplateParts ? (defaultTemplateParts ?? _libraryPathTemplatePartResponseFixture.CreateMany(2)) : []);
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryPathTemplateCatalogResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryPathTemplateCatalogResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
