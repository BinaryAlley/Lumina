#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Command for updating the settings of the current user.
/// </summary>
/// <param name="IsPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
/// <param name="ItemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
/// <param name="IgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
public record UpdateUserSettingsCommand(
    bool IsPaginationEnabled,
    int ItemsPerPage,
    bool IgnoreThePrefixForAlphaPicker
) : ICommand;
