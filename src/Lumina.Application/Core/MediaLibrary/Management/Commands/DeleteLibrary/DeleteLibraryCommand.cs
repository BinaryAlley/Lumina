#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;

/// <summary>
/// Command for deleting a media library by its Id.
/// </summary>
/// <param name="Id">The unique identifier of the media library to delete.</param>
[DebuggerDisplay("Id: {Id}")]
public record DeleteLibraryCommand(
    Guid Id
) : ICommand;
