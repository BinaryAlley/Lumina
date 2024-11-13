#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.UsersManagement;

/// <summary>
/// Model for verifying the initialization status of the application.
/// </summary>
[DebuggerDisplay("IsInitialized: {IsInitialized}")]
public class InitializationModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the application has been run before, and the super admin account has been created.
    /// </summary>
    public bool IsInitialized { get; set; }
}
