#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the editions of a work from the Open Library API.
/// </summary>
internal sealed record OpenLibraryEditionsResponse
{
    /// <summary>
    /// Gets the editions of the work.
    /// </summary>
    [JsonPropertyName("entries")]
    public List<OpenLibraryEditionResponse> Entries { get; init; } = [];
}
