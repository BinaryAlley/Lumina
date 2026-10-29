#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Statistics;

/// <summary>
/// Represents a request to delete the resume points of a user.
/// </summary>
/// <param name="UserId">The Id of the user whose resume points are deleted. Optional; when omitted, the current user is used.</param>
/// <param name="LibraryId">The Id of the media library to restrict the deletion to. Optional.</param>
[DebuggerDisplay("UserId: {UserId}; LibraryId: {LibraryId}")]
public sealed record ClearResumePointsRequest(
    string? UserId,
    string? LibraryId
);
