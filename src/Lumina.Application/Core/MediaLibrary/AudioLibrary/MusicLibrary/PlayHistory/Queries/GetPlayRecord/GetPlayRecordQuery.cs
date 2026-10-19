#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetPlayRecord;

/// <summary>
/// Query for getting a recorded play of a track by the current user.
/// </summary>
/// <param name="PlayRecordId">The Id of the play record, taken from the route.</param>
[DebuggerDisplay("PlayRecordId: {PlayRecordId}")]
public record GetPlayRecordQuery(
    string? PlayRecordId
) : IQuery;
