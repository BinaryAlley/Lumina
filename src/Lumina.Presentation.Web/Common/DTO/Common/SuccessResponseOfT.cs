#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Response data transfer object for the JSON responses of the web application that carry a payload.
/// </summary>
/// <typeparam name="TData">The type of the payload of the response.</typeparam>
/// <param name="Success">Whether the operation succeeded or not.</param>
/// <param name="Data">The payload of the response.</param>
public record SuccessResponse<TData>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("data")] TData? Data
);
