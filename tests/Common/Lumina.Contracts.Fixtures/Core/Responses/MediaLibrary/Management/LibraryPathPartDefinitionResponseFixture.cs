#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryPathPartDefinitionResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathPartDefinitionResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryPathPartDefinitionResponse"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of the path part.</param>
    /// <param name="valueType">Optional. The value type captured by the path part.</param>
    /// <param name="defaultRepresentation">Optional. The default representation of the path part.</param>
    /// <param name="isOptionalByDefault">Whether the path part is optional by default.</param>
    /// <returns>The created <see cref="LibraryPathPartDefinitionResponse"/>.</returns>
    public LibraryPathPartDefinitionResponse Create(
        string? kind = null,
        string? valueType = null,
        string? defaultRepresentation = null,
        bool isOptionalByDefault = false)
    {
        return new LibraryPathPartDefinitionResponse(
            kind ?? "Artist",
            valueType ?? "Text",
            defaultRepresentation ?? "{0}",
            isOptionalByDefault);
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryPathPartDefinitionResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryPathPartDefinitionResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
