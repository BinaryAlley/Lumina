#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.Responses.UsersManagement;

/// <summary>
/// Model for verifying the initialization status of the application.
/// </summary>
[DebuggerDisplay("IsInitialized: {IsInitialized}")]
public class InitializationResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the application has been run before, and the super admin account has been created.
    /// </summary>
    public bool IsInitialized { get; set; }
}
