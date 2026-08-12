#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for an author reference within a work from the Open Library API.
/// </summary>
internal sealed record OpenLibraryWorkAuthorResponse
{
    /// <summary>
    /// Gets the author of the work.
    /// </summary>
    [JsonPropertyName("author")]
    public OpenLibraryKeyReferenceResponse? Author { get; init; }

    /// <summary>
    /// Gets the key of the author reference.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; } // Older records can contain a direct key rather than an author wrapper.
}
