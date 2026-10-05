namespace Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;

/// <summary>
/// Enumeration for the value types captured by the typed parts of a media library path template.
/// </summary>
public enum LibraryPathValueType
{
    /// <summary>
    /// The part carries no captured value, like a literal or a path separator.
    /// </summary>
    None,

    /// <summary>
    /// The part captures a free text value.
    /// </summary>
    Text,

    /// <summary>
    /// The part captures an integer value.
    /// </summary>
    Integer,

    /// <summary>
    /// The part captures a four digit year.
    /// </summary>
    Year,

    /// <summary>
    /// The part captures a value that is later resolved against an enumeration.
    /// </summary>
    Enum
}
