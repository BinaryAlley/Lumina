#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Queries.GetPlaylists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Queries.GetPlaylists;

/// <summary>
/// Fixture class for the <see cref="GetPlaylistsQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistsQueryFixture
{
    /// <summary>
    /// Creates a <see cref="GetPlaylistsQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="GetPlaylistsQuery"/>.</returns>
    /// <remarks>
    /// The query carries no members, because the current user is always taken from the authenticated request context.
    /// </remarks>
    public GetPlaylistsQuery Create()
    {
        return new GetPlaylistsQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetPlaylistsQuery"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetPlaylistsQuery"/> instances.</returns>
    public List<GetPlaylistsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
