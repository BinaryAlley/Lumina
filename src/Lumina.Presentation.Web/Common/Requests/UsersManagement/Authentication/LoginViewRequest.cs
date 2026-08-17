#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.UsersManagement.Authentication;

/// <summary>
/// Represents the request for displaying the account login view.
/// </summary>
/// <param name="ReturnUrl">The URL to return to, after login.</param>
[DebuggerDisplay("ReturnUrl: {ReturnUrl}")]
public record LoginViewRequest(
    string? ReturnUrl = null
);
