#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Entities;

/// <summary>
/// Entity for a season.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class Season : Entity<SeasonId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Season"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the season.</param>
    public Season(SeasonId id) : base(id)
    {
    }
}
