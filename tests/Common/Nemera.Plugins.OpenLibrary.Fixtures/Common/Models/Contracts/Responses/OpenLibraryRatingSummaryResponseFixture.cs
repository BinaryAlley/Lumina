#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="OpenLibraryRatingSummaryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibraryRatingSummaryResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="OpenLibraryRatingSummaryResponse"/>.
    /// </summary>
    /// <param name="average">Optional. The average rating of the work.</param>
    /// <param name="count">Optional. The number of ratings of the work.</param>
    /// <returns>The created rating summary response.</returns>
    public OpenLibraryRatingSummaryResponse Create(decimal? average = null, int? count = null)
    {
        return new OpenLibraryRatingSummaryResponse
        {
            Average = average ?? 4.5m,
            Count = count ?? 100
        };
    }
}
