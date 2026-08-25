#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="ArtworkDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtworkDtoFixture
{
    /// <summary>
    /// Creates an <see cref="ArtworkDto"/>.
    /// </summary>
    /// <param name="localPath">Optional. The local file system path of the artwork.</param>
    /// <param name="remoteUrl">Optional. The remote URL of the artwork.</param>
    /// <returns>The created <see cref="ArtworkDto"/>.</returns>
    public ArtworkDto Create(
        string? localPath = null, 
        string? remoteUrl = null)
    {
        return new ArtworkDto(localPath, remoteUrl);
    }

    /// <summary>
    /// Creates a list of <see cref="ArtworkDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ArtworkDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
