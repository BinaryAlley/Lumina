#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetResumePoint;

/// <summary>
/// Query for getting the resume point of the current user for a media item.
/// </summary>
/// <param name="MediaKind">The kind of media to resume, taken from the route.</param>
/// <param name="MediaItemId">The Id of the media item to resume, taken from the route.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public record GetResumePointQuery(
    string? MediaKind,
    string? MediaItemId
) : IQuery;
