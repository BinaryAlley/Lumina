#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Contracts.Responses.Authentication;

/// <summary>
/// Represents the response model for an account password reset request.
/// </summary>
/// <param name="IsPasswordReset">Whether the password was reset or not.</param>
[DebuggerDisplay("IsPasswordReset: {IsPasswordReset}")]
public record RecoverPasswordResponse(
    bool IsPasswordReset
);
