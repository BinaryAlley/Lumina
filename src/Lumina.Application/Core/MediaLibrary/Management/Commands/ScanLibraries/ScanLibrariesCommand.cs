#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Command for initiating the scan of all media libraries.
/// </summary>
public record ScanLibrariesCommand() : IRequest<ErrorOr<IEnumerable<ScanLibraryResponse>>>;
