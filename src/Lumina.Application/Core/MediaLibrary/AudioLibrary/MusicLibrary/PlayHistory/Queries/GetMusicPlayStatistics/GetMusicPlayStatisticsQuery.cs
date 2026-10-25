#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetMusicPlayStatistics;

/// <summary>
/// Query for getting the listening statistics of the current user.
/// </summary>
[DebuggerDisplay("Music play statistics")]
public record GetMusicPlayStatisticsQuery() : IQuery;
