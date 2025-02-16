#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.Infrastructure.Models.Scanning;

/// <summary>
/// Model for tracking media library scans.
/// </summary>
/// <param name="ScanId">The unique identifier of the media library scan to be tracked.</param>
/// <param name="UserId">The unique identifier of the username who initiated the media library scan.</param>
public record MediaScanTrackerModel(Guid ScanId, Guid UserId);
