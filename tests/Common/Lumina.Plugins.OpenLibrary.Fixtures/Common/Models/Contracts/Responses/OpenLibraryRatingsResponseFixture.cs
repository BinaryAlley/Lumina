#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="OpenLibraryRatingsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibraryRatingsResponseFixture
{
    private readonly OpenLibraryRatingSummaryResponseFixture _ratingSummaryResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="OpenLibraryRatingsResponse"/>.
    /// </summary>
    /// <param name="summary">Optional. The summary of the ratings of the work.</param>
    /// <returns>The created ratings response.</returns>
    public OpenLibraryRatingsResponse Create(OpenLibraryRatingSummaryResponse? summary = null)
    {
        return new OpenLibraryRatingsResponse
        {
            Summary = summary ?? _ratingSummaryResponseFixture.Create()
        };
    }
}
