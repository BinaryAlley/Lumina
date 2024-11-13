#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate;

/// <summary>
/// Entity for a TV show.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class TvShow : Entity<TvShowId>
{

}
