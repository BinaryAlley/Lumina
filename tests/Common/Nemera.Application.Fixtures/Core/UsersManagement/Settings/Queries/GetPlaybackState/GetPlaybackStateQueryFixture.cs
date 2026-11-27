#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.UsersManagement.Settings.Queries.GetPlaybackState;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Settings.Queries.GetPlaybackState;

/// <summary>
/// Fixture class for the <see cref="GetPlaybackStateQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateQueryFixture
{
    /// <summary>
    /// Creates a new <see cref="GetPlaybackStateQuery"/> instance.
    /// </summary>
    /// <returns>The created <see cref="GetPlaybackStateQuery"/>.</returns>
    public GetPlaybackStateQuery Create()
    {
        return new GetPlaybackStateQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="GetPlaybackStateQuery"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetPlaybackStateQuery"/> instances.</returns>
    public List<GetPlaybackStateQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
