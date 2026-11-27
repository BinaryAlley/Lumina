#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Infrastructure.Common.Models.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing database configuration settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class DatabaseSettingsDto
{
    /// <summary>
    /// The name of the configuration section that holds the database connection strings.
    /// </summary>
    public const string SECTION_NAME = "ConnectionStrings";

    /// <summary>
    /// Gets or sets the connection string to the database.
    /// </summary>
    public required string DefaultConnection { get; init; }
}
