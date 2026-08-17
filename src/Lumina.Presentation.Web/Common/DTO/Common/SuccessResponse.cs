#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Response data transfer object for the JSON responses of the web application that do not carry a payload.
/// </summary>
/// <param name="Success">Whether the operation succeeded or not.</param>
public record SuccessResponse(
    [property: JsonPropertyName("success")] bool Success
);
