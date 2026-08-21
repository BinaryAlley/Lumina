#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
#endregion

namespace Lumina.Application.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Extension methods for converting <see cref="UpdateUserSettingsRequest"/>.
/// </summary>
public static class UpdateUserSettingsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateUserSettingsCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static UpdateUserSettingsCommand ToCommand(this UpdateUserSettingsRequest request)
    {
        return new UpdateUserSettingsCommand(
            request.IsPaginationEnabled,
            request.ItemsPerPage,
            request.IgnoreThePrefixForAlphaPicker,
            request.IsThemeCachingEnabled);
    }
}
