#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="OtherMetadataLookupDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class OtherMetadataLookupDtoFixture
{
    /// <summary>
    /// Creates a <see cref="OtherMetadataLookupDto"/>.
    /// </summary>
    /// <returns>The created <see cref="OtherMetadataLookupDto"/>.</returns>
    public OtherMetadataLookupDto Create()
    {
        return new OtherMetadataLookupDto();
    }

    /// <summary>
    /// Creates a list of <see cref="OtherMetadataLookupDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<OtherMetadataLookupDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
