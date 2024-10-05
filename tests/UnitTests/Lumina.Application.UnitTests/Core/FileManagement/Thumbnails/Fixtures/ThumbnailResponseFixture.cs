#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Contracts.Responses.FileManagement;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Thumbnails.Fixtures;

/// <summary>
/// Fixture class for the <see cref="ThumbnailResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailResponseFixture
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid query to get a thumbnail.
    /// </summary>
    /// <returns>The created query.</returns>
    public static ThumbnailResponse CreateThumbnailResponse()
    {
        return new Faker<ThumbnailResponse>()
            .CustomInstantiator(f => new ThumbnailResponse(
                default,
                default!
            ))
            .RuleFor(x => x.Type, f => f.PickRandom<ImageType>())
            .RuleFor(x => x.Bytes, f => f.Random.Bytes(20));
    }
    #endregion
}
