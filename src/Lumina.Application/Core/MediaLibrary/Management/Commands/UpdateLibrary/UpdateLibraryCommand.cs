#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;

/// <summary>
/// Command for updating a media library.
/// </summary>
/// <param name="Id">The Id of the media library.</param>
/// <param name="OwnerId">The Id of the user owning the media library.</param>
/// <param name="CurrentUserId">The Id of the user making the request to update the media library.</param>
/// <param name="LibraryType">The type of the media library.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located.</param>
/// <param name="Title">The title of the media library.</param>
[DebuggerDisplay("Title: {Title}")]
public record UpdateLibraryCommand(
    Guid Id,
    Guid OwnerId,
    Guid CurrentUserId,
    string? Title,
    string? LibraryType,
    string[]? ContentLocations
) : IRequest<ErrorOr<LibraryResponse>>;