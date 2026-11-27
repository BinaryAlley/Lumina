#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;

/// <summary>
/// Command for canceling the previously started scan of all media libraries.
/// </summary>
public record CancelLibrariesScanCommand : ICommand;
