#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.UsersManagement.Settings;

/// <summary>
/// Represents the settings of a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user that owns these settings.</param>
/// <param name="IsPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
/// <param name="ItemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
/// <param name="IgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
[DebuggerDisplay("UserId: {UserId}")]
public sealed record UserSettingsResponse(
    Guid UserId,
    bool IsPaginationEnabled,
    int ItemsPerPage,
    bool IgnoreThePrefixForAlphaPicker
);
