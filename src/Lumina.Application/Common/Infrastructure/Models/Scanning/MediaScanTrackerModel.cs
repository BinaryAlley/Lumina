#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.Infrastructure.Models.Scanning;

/// <summary>
/// Model for tracking media library scans.
/// </summary>
/// <param name="ScanId">The Id of the media library scan to be tracked.</param>
/// <param name="UserId">The Id of the username who initiated the media library scan.</param>
public record MediaScanTrackerModel(Guid ScanId, Guid UserId);
