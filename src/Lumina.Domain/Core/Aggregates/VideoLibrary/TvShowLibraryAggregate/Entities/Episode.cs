#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.ValueObjects;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.Entities;

/// <summary>
/// Entity for an episode.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class Episode : Entity<EpisodeId>
{

}