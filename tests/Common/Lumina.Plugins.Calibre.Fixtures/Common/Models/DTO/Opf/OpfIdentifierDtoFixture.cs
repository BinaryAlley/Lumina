#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.Calibre.Fixtures.Common.Models.DTO.Opf;

/// <summary>
/// Fixture class for the <see cref="OpfIdentifierDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class OpfIdentifierDtoFixture
{
    /// <summary>
    /// Creates an <see cref="OpfIdentifierDto"/>.
    /// </summary>
    /// <param name="scheme">The scheme of the identifier, for example <c>ISBN</c> or <c>GOODREADS</c>.</param>
    /// <param name="value">The value of the identifier.</param>
    /// <returns>The created <see cref="OpfIdentifierDto"/>.</returns>
    public OpfIdentifierDto Create(
        string scheme = "ISBN", 
        string value = "9780306406157")
    {
        return new OpfIdentifierDto(scheme, value);
    }

    /// <summary>
    /// Creates a list of <see cref="OpfIdentifierDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<OpfIdentifierDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
