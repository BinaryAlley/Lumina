#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.UsersManagement;

/// </summary>
/// Represents a response for verifying the initialization status of the application.
/// </summary>
/// <param name="IsInitialized">Indicates whether the application has been run before, and the super admin account has been created.</param>
[DebuggerDisplay("IsInitialized: {IsInitialized}")]
public record InitializationResponse(
    bool IsInitialized 
);
