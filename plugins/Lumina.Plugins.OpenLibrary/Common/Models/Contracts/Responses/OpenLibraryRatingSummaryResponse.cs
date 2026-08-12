#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the summary of the ratings of a work from the Open Library API.
/// </summary>
internal sealed record OpenLibraryRatingSummaryResponse
{
    /// <summary>
    /// Gets the average rating of the work.
    /// </summary>
    [JsonPropertyName("average")]
    public decimal? Average { get; init; }

    /// <summary>
    /// Gets the number of ratings of the work.
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; init; }
}
