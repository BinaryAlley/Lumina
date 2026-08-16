#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.UsersManagement.Settings;

/// <summary>
/// Represents a request to update the settings of the current user.
/// </summary>
/// <param name="IsPaginationEnabled">Whether pagination is enabled for the user, or not. Required.</param>
/// <param name="ItemsPerPage">The number of library items displayed per page when pagination is enabled. Required.</param>
/// <param name="IgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not. Required.</param>
[DebuggerDisplay("IsPaginationEnabled: {IsPaginationEnabled}; ItemsPerPage: {ItemsPerPage}")]
public sealed record UpdateUserSettingsRequest(
    bool IsPaginationEnabled,
    int ItemsPerPage,
    bool IgnoreThePrefixForAlphaPicker
);
