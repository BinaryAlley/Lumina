#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request to delete the resume points of a user.
/// </summary>
/// <param name="LibraryId">The Id of the media library to restrict the deletion to. Optional.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public sealed record ClearResumePointsRequest(
    string? LibraryId
);
