#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a search from the Open Library API.
/// </summary>
internal sealed record OpenLibrarySearchResponse
{
    /// <summary>
    /// Gets the documents that match the search.
    /// </summary>
    [JsonPropertyName("docs")]
    public List<OpenLibrarySearchDocumentResponse> Documents { get; init; } = [];
}
