#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.Playlists.Queries.GetPlaylists;

/// <summary>
/// Query for getting the playlists of the current user.
/// </summary>
public record GetPlaylistsQuery : IQuery;
