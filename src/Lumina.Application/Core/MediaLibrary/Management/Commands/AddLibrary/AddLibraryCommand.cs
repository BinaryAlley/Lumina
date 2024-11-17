#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;

/// <summary>
/// Command for adding a media library.
/// </summary>
/// <param name="UserId">The Id of the user owning the media library.</param>
/// <param name="LibraryType">The type of the media library.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located.</param>
/// <param name="Title">The title of the media library.</param>
[DebuggerDisplay("Title: {Title}")]
public record AddLibraryCommand(
    Guid UserId,
    string? Title,
    string? LibraryType,
    string[]? ContentLocations
) : IRequest<ErrorOr<LibraryResponse>>;
