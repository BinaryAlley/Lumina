#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for a reference to a resource of the Open Library API by its key.
/// </summary>
internal sealed record OpenLibraryKeyReferenceResponse
{
    /// <summary>
    /// Gets the key of the referenced resource.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }
}
