#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.Configuration;

/// <summary>
/// Model for deserializing database configuration settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class DatabaseSettingsModel
{
    public const string SECTION_NAME = "ConnectionStrings";

    /// <summary>
    /// Gets or sets the connection string to the database.
    /// </summary>
    public required string DefaultConnection { get; init; }
}
