#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Entities;

/// <summary>
/// Entity for an episode.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class Episode : Entity<EpisodeId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Episode"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the episode.</param>
    public Episode(EpisodeId id) : base(id)
    {
    }
}
