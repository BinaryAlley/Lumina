#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

/// <summary>
/// Command for cancelling the scan of a media library by its Id.
/// </summary>
/// <param name="Id">The Id of the media library whose scan is cancelled.</param>
[DebuggerDisplay("Id: {Id}")]
public record CancelLibraryScanCommand(
    Guid Id
) : IRequest<ErrorOr<Success>>;
