#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Command for initiating the scan of a media library.
/// </summary>
/// <param name="Id">The Id of the library to scan.</param>
public record ScanLibraryCommand(
    Guid Id    
) : IRequest<ErrorOr<ScanLibraryResponse>>;
