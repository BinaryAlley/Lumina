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
    public Season(SeasonId id) : base(id)
    {
    }
}
