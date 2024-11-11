#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authentication;
using Mediator;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;

/// <summary>
/// Query for authenticating an account.
/// </summary>
/// <param name="Username">The username of the account to authenticate.</param>
/// <param name="Password">The password of the account to authenticate.</param>
/// <param name="TotpCode">The TOTP (Time-Based One-Time Password) of the account to authenticate.</param>
[DebuggerDisplay("Path: {Path}")]
public record LoginUserQuery(
    string? Username,
    string? Password,
    string? TotpCode = null
) : IRequest<ErrorOr<LoginResponse>>;
