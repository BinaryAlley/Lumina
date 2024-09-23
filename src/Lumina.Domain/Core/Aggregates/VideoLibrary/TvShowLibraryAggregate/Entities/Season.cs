#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.ValueObjects;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.Entities;

/// <summary>
/// Entity for a season.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class Season : Entity<SeasonId>
{

}