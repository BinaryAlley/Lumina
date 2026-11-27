#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.UsersManagement.Settings.Queries.GetPlaybackState;

/// <summary>
/// Query for getting the playback state the player reported for the current user automatically. These values are not part of the settings the user edits.
/// </summary>
public record GetPlaybackStateQuery : IQuery;
