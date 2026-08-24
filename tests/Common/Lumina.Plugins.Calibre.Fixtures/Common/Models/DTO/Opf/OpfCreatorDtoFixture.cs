#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.Calibre.Fixtures.Common.Models.DTO.Opf;

/// <summary>
/// Fixture class for the <see cref="OpfCreatorDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class OpfCreatorDtoFixture
{
    /// <summary>
    /// Creates an <see cref="OpfCreatorDto"/>.
    /// </summary>
    /// <param name="name">The name of the creator.</param>
    /// <param name="role">The role code of the creator, for example <c>aut</c> for author.</param>
    /// <returns>The created <see cref="OpfCreatorDto"/>.</returns>
    public OpfCreatorDto Create(
        string name = "Test Creator", 
        string? role = null)
    {
        return new OpfCreatorDto(name, role);
    }

    /// <summary>
    /// Creates a list of <see cref="OpfCreatorDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<OpfCreatorDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
