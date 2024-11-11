#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.UsersManagement;

/// <summary>
/// Repository entity for a user.
/// </summary>
[DebuggerDisplay("Username: {Username}")]
public class UserEntity : IStorageEntity
{
    /// <summary>
    /// Gets the Id of the user.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the temporary hashed password used in password recovery.
    /// </summary>
    public string? TempPassword { get; set; }

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
    /// Gets or sets the date and time when the temporary password used in password recovery was created.
    /// </summary>
    public DateTime? TempPasswordCreated { get; set; }

    /// <summary>
    /// Gets the collection of libraries owned by this user.
    /// </summary>
    public required ICollection<LibraryEntity> Libraries { get; init; } = [];

    /// <summary>
    /// Gets the time and date when the entity was added.
    /// </summary>
    public required DateTime Created { get; set; }

    /// <summary>
    /// Gets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? Updated { get; set; }
}
