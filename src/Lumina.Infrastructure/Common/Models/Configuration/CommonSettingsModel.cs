namespace Lumina.Infrastructure.Common.Models.Configuration;

/// <summary>
/// Model for deserializing shared application configuration settings.
/// </summary>
public class CommonSettingsModel
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public const string SectionName = "CommonSettings";
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the theme of the application.
    /// </summary>
    public required string Theme { get; init; }
    #endregion
}