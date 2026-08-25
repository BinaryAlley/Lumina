#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.Calibre.Fixtures.Common.Models.DTO.Opf;

/// <summary>
/// Fixture class for the <see cref="OpfContributorDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class OpfContributorDtoFixture
{
    /// <summary>
    /// Creates an <see cref="OpfContributorDto"/>.
    /// </summary>
    /// <param name="name">The name of the contributor.</param>
    /// <param name="role">The role code of the contributor, for example <c>bkp</c> for the book producer.</param>
    /// <returns>The created <see cref="OpfContributorDto"/>.</returns>
    public OpfContributorDto Create(
        string name = "Test Contributor", 
        string? role = null)
    {
        return new OpfContributorDto(name, role);
    }

    /// <summary>
    /// Creates a list of <see cref="OpfContributorDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<OpfContributorDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
