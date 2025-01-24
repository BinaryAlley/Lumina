#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Mediator;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Command for initiating the scan of all media libraries.
/// </summary>
public record ScanLibrariesCommand() : IRequest<ErrorOr<Success>>;
