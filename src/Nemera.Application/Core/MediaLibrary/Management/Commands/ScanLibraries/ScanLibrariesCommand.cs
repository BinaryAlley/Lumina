#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Command for initiating the scan of all media libraries.
/// </summary>
public record ScanLibrariesCommand() : ICommand;
