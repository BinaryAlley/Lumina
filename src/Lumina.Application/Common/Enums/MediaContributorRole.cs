#region ========================================================================= USING =====================================================================================
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Application.Common.Enums;

/// <summary>
/// Enumeration for the various roles of media contributors across different types of media.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaContributorRole
{
    Actor,
    Director,
    Singer,
    Author, 
}