namespace Lumina.Infrastructure.Common.Models.Configuration;

/// <summary>
/// Model for deserializing database configuration settings.
/// </summary>
public class DatabaseSettingsModel
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public const string SectionName = "ConnectionStrings";
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the connection string to the database.
    /// </summary>
    public required string DefaultConnection { get; init; }
    #endregion
}