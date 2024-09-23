#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.ValueObjects;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate;

/// <summary>
/// Entity for a TV show.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class TvShow : Entity<TvShowId>
{

}