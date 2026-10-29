#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Statistics;

/// <summary>
/// Represents a request to delete the consumption history of a user.
/// </summary>
/// <param name="UserId">The Id of the user whose history is deleted. Optional; when omitted, the current user is used.</param>
[DebuggerDisplay("UserId: {UserId}")]
public sealed record ClearConsumptionHistoryRequest(
    string? UserId
);
