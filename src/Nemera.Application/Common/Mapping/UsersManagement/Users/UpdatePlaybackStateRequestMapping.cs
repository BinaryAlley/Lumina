#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using Nemera.Contracts.Requests.UsersManagement.Settings;
#endregion

namespace Nemera.Application.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Extension methods for converting <see cref="UpdatePlaybackStateRequest"/>.
/// </summary>
public static class UpdatePlaybackStateRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdatePlaybackStateCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static UpdatePlaybackStateCommand ToCommand(this UpdatePlaybackStateRequest request)
    {
        return new UpdatePlaybackStateCommand(
            request.Volume,
            request.IsMuted
        );
    }
}
