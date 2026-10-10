#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;

/// <summary>
/// Fixture class for the <see cref="GetLibraryPathTemplatePartsQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryPathTemplatePartsQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryPathTemplatePartsQuery"/>.
    /// </summary>
    /// <param name="libraryType">Optional. The media library type whose available path parts are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryPathTemplatePartsQuery"/>.</returns>
    public GetLibraryPathTemplatePartsQuery Create(string? libraryType = null)
    {
        return new GetLibraryPathTemplatePartsQuery(libraryType ?? "Music");
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryPathTemplatePartsQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryPathTemplatePartsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
