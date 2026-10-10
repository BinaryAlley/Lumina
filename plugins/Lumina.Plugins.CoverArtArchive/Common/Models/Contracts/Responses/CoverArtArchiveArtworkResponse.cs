#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;

/// <summary>
/// Represents a response for the artwork of a release or a release group from the Cover Art Archive API.
/// </summary>
internal sealed record CoverArtArchiveArtworkResponse
{
    /// <summary>
    /// Gets the images of the artwork. It is nullable because the deserializer writes a JSON null over the initializer, and may also contain null elements.
    /// </summary>
    [JsonPropertyName("images")]
    public List<CoverArtArchiveImageResponse?>? Images { get; init; }
}
