#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Entities.UsersManagement;

/// <summary>
/// Repository entity for a user.
/// </summary>
[DebuggerDisplay("Username: {Username}")]
public class UserEntity : IStorageEntity
{
    /// <summary>
    /// Gets the Id of the user.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the optional Time-based One-Time Password (TOTP) secret for two-factor authentication.
    /// </summary>
    public string? TotpSecret { get; set; }

    /// <summary>
    /// Gets or sets the optional token used for account verification or password reset.
    /// </summary>
    public string? VerificationToken { get; set; }

    /// <summary>
    /// Gets or sets the optional date and time when the verification token was created.
    /// </summary>
    public DateTime? VerificationTokenCreated { get; set; }

    /// <summary>
    /// Gets the time and date when the entity was added.
    /// </summary>
    public required DateTime Created { get; set; }

    /// <summary>
    /// Gets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? Updated { get; set; }
}
