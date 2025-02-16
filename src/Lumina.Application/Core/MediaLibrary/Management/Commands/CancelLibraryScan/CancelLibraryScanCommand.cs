#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

/// <summary>
/// Command for canceling the scan of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose scan is cancelled.</param>
/// <param name="ScanId">The unique identifier of the scan to cancel.</param>
[DebuggerDisplay("LibraryId: {LibraryId}; ScanId: {ScanId}")]
public record CancelLibraryScanCommand(
    Guid LibraryId,
    Guid ScanId
) : IRequest<ErrorOr<Success>>;
