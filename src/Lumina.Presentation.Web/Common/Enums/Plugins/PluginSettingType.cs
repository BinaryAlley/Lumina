namespace Lumina.Presentation.Web.Common.Enums.Plugins;

/// <summary>
/// Enumeration for the types of a plugin setting.
/// </summary>
public enum PluginSettingType
{
    /// <summary>
    /// A free-form single line text value.
    /// </summary>
    Text,

    /// <summary>
    /// A numeric value.
    /// </summary>
    Number,

    /// <summary>
    /// A boolean (true/false) value.
    /// </summary>
    Boolean,

    /// <summary>
    /// A value selected from a list of allowed values.
    /// </summary>
    Select
}
