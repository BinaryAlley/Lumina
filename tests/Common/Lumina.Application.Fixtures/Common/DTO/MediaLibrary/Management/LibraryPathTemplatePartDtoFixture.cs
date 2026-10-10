#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryPathTemplatePartDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplatePartDtoFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryPathTemplatePartDto"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of the path part.</param>
    /// <param name="representation">Optional. The literal text or the value mask of the path part.</param>
    /// <param name="isOptional">Whether the path part can be absent from the path.</param>
    /// <returns>The created <see cref="LibraryPathTemplatePartDto"/>.</returns>
    public LibraryPathTemplatePartDto Create(
        string? kind = null,
        string? representation = null,
        bool isOptional = false)
    {
        return new LibraryPathTemplatePartDto(
            kind ?? "Artist",
            representation ?? "{0}",
            isOptional);
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryPathTemplatePartDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryPathTemplatePartDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
