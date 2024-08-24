#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Application.Common.Enums;

/// <summary>
/// Enumeration for the various sources of book ratings.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookRatingSource
{
    User,
    Goodreads,
    Amazon,
    GoogleBooks,
}