#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Application.Common.Enums;

/// <summary>
/// Enumeration for the different ISBN (International Standard Book Number) formats.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IsbnFormat
{
    Isbn10,
    Isbn13
}