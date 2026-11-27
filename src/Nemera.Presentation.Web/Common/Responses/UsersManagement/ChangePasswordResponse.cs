#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.Responses.UsersManagement;

/// <summary>
/// Represents the response model for an account password change request.
/// </summary>
/// <param name="IsPasswordChanged">Whether the password was changed or not.</param>
[DebuggerDisplay("IsPasswordChanged: {IsPasswordChanged}")]
public record ChangePasswordResponse(
    bool IsPasswordChanged
);
