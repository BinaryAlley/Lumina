#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for an author from the Open Library API.
/// </summary>
internal sealed record OpenLibraryAuthorResponse
{
    /// <summary>
    /// Gets the key of the author.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>
    /// Gets the name of the author.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the personal name of the author.
    /// </summary>
    [JsonPropertyName("personal_name")]
    public string? PersonalName { get; init; }
}
