#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryPathTemplatePartRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplatePartRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryPathTemplatePartRequest"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of the path part.</param>
    /// <param name="representation">Optional. The literal text or the value mask of the path part.</param>
    /// <param name="isOptional">Whether the path part can be absent from the path.</param>
    /// <returns>The created <see cref="LibraryPathTemplatePartRequest"/>.</returns>
    public LibraryPathTemplatePartRequest Create(
        string? kind = null,
        string? representation = null,
        bool isOptional = false)
    {
        return new LibraryPathTemplatePartRequest(
            kind ?? "Artist",
            representation ?? "{0}",
            isOptional);
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryPathTemplatePartRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryPathTemplatePartRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
