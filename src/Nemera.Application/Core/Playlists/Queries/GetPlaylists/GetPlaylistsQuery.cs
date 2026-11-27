#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.Playlists.Queries.GetPlaylists;

/// <summary>
/// Query for getting the playlists of the current user.
/// </summary>
public record GetPlaylistsQuery : IQuery;
