#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryPathTemplatePartResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplatePartResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryPathTemplatePartResponse"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of the path part.</param>
    /// <param name="representation">Optional. The literal text or the value mask of the path part.</param>
    /// <param name="isOptional">Whether the path part can be absent from the path.</param>
    /// <returns>The created <see cref="LibraryPathTemplatePartResponse"/>.</returns>
    public LibraryPathTemplatePartResponse Create(
        string? kind = null,
        string? representation = null,
        bool isOptional = false)
    {
        return new LibraryPathTemplatePartResponse(
            kind ?? "Artist",
            representation ?? "{0}",
            isOptional);
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryPathTemplatePartResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryPathTemplatePartResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
