#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Models.Core;
using Nemera.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System.Diagnostics;
#endregion

namespace Nemera.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate;

/// <summary>
/// Entity for a TV show.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class TvShow : Entity<TvShowId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TvShow"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the TV show.</param>
    public TvShow(TvShowId id) : base(id)
    {
    }
}
