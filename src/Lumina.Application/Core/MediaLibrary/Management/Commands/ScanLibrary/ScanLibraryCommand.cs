#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Command for initiating the scan of a media library.
/// </summary>
/// <param name="Id">The unique identifier of the library to scan.</param>
public record ScanLibraryCommand(
    Guid Id    
) : ICommand;
