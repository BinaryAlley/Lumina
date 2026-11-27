#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;

/// <summary>
/// Represents the request model for account password recovery.
/// </summary>
/// <param name="Username">The username of the account for which to recover the password.</param>
/// <param name="TotpCode">The TOTP (Time-Based One-Time Password) of the account for which to recover the password.</param>
[DebuggerDisplay("Username: {Username}")]
public record class RecoverPasswordCommand(
    string? Username,
    string? TotpCode
) : ICommand;
