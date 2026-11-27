#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;

/// <summary>
/// Command for canceling the previously started scan of all media libraries.
/// </summary>
public record CancelLibrariesScanCommand : ICommand;
