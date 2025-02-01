#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Mediator;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;

/// <summary>
/// Command for cancelling the previously started scan of all media libraries.
/// </summary>
public record CancelLibrariesScanCommand : IRequest<ErrorOr<Success>>;
