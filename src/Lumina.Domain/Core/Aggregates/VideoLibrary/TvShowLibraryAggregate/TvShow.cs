#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.ValueObjects;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate;

/// <summary>
/// Entity for a TV show.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class TvShow : Entity<TvShowId>
{

}