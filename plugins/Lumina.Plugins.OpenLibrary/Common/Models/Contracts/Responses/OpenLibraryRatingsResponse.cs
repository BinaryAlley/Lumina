#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the ratings of a work from the Open Library API.
/// </summary>
internal sealed record OpenLibraryRatingsResponse
{
    /// <summary>
    /// Gets the summary of the ratings of the work.
    /// </summary>
    [JsonPropertyName("summary")]
    public OpenLibraryRatingSummaryResponse? Summary { get; init; }
}
